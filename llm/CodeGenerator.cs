using LLMing.LLM;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace LLMing.llm;

/// <summary>
/// We're using an LLM to generate answers via code. That requires us to run something "it" generated.
/// This class attempts to run AI generated code safely enough so that it cannot nuke your PC.
/// 
/// WARNING: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
///          !! IF THE LLM NUKES YOUR PC, IT IS YOUR OWN FAULT. SORRY, I CANNOT BE HELD RESPONSIBLE. !!
/// WARNING: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
/// 
/// This class attempts to protect against malicious code. But if it returns code damages your computer, it is ___your___ fault. 
/// 
/// It should not be able to because:
/// - it limits the libraries included (safe ones only), but sanitisation is easier said than done.
/// - it does not include the most dangerous DLLs.
/// - it is kept in a different namespace to minimise (segregated).
/// - no assembly can be sideloaded by it (sandboxed).
/// - scans for dangerous code (not foolproof, but at least it tries).
/// 
/// That said, use at your peril.
/// </summary>
internal static class CodeGenerator
{
    /// <summary>
    /// Used to separate the rule into lines. I should debate Environment.NewLine vs \n.
    /// </summary>
    private static readonly string[] c_constSeparator = ["\n"];

    /// <summary>
    /// Indicates text following is generated c# code.
    /// </summary>
    private const string c_csharpStartMarker = "```csharp";

    /// <summary>
    /// Indicates end of generated c# code.
    /// </summary>
    private const string c_csharpEndMarker = "```";

    /// <summary>
    /// Use for the LIKE, to save the answer
    /// </summary>
    private static string s_lastAnswerFunction = "";

    /// <summary>
    /// Returns the last answer "function" (what the AI generated) that was run.
    /// </summary>
    internal static string LastAnswerFunction => s_lastAnswerFunction;

    /// <summary>
    /// Tracks the last compilation message. (Useful for UI/debugging).
    /// </summary>
    internal static string LastCompilationMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Contains the assembly reference (library) after compilation.
    /// </summary>
    private static Assembly? s_assembly;

    /// <summary>
    /// Contains the compiled code in memory.
    /// </summary>
    private static MemoryStream? s_compiledCodememoryStream;

    /// <summary>
    /// Contains the assembly load context.
    /// </summary>
    private static AssemblyLoadContext? s_assemblyLoadContext;

    /// <summary>
    /// Attempt to compile and execute the AI generated code.
    /// The LLM sometimes spews out the reason, on other occasions it just gives the code, so we
    /// play find the code and extract it.
    /// </summary>
    /// <param name="aiGeneratedResponse">The AI response that should have code embedded.</param>
    /// <param name="codeRuns">indicates whether code generated runs</param>
    /// <returns></returns>
    internal static string CompileAndExecute(string aiGeneratedResponse, out bool codeRuns)
    {
        s_lastAnswerFunction = "";

        // The AI should output the thought process (if you can call it that)
        // embedded within should be ```csharp {generated code} ```.
        // It extracts the c# code, and executes it, returning the answer.

        if (!aiGeneratedResponse.Contains("```csharp") && !aiGeneratedResponse.Contains("public static class"))
        {
            codeRuns = true;
            return aiGeneratedResponse;
        }

        // extract the c# code from the response.
        aiGeneratedResponse = GetCSharpCodeFromResponse(aiGeneratedResponse);

        // store the last answer function, so we can use it in the future.
        s_lastAnswerFunction = aiGeneratedResponse;

        if (aiGeneratedResponse.Contains("answer()"))
        {
            // do some rudimentary checks to see if it is safe to run.
            if (SecurityChecksSuggestDontRun(ref aiGeneratedResponse))
            {
                codeRuns = false;
                return "Sorry, the answer() method was deemed unsafe to run.";
            }

            // attempt to compile the code, and if it compiles, we run it.
            if (AICodeCompiledCorrectly(ref aiGeneratedResponse))
            {
                codeRuns = true;
                return RunAICodeToGetAnswer(aiGeneratedResponse);
            }
        }

        s_lastAnswerFunction = "";

        // code did not compile correctly, so we didn't run it
        codeRuns = false;

        return LastCompilationMessage;
    }

    /// <summary>
    /// Apply a list of safety checks.
    /// THIS IS NOT EXHAUSTIVE. IT PROVIDES RUDIMENTARY PROTECTION, IN ADDITION TO SANDBOXING.
    /// </summary>
    /// <param name="aiGeneratedResponse"></param>
    /// <returns>true if deemed unsafe | false means it passes rudimentary checks, but still could be malicious.</returns>
    private static bool SecurityChecksSuggestDontRun(ref string aiGeneratedResponse)
    {
        // you can do some nasty exploits using comments. So we remove them.

        // remove /* */ comments from the code.
        aiGeneratedResponse = System.Text.RegularExpressions.Regex.Replace(aiGeneratedResponse, "/\\*.*?\\*/", "", System.Text.RegularExpressions.RegexOptions.Singleline);

        // remove // comments from the code.
        aiGeneratedResponse = System.Text.RegularExpressions.Regex.Replace(aiGeneratedResponse, "//.*", "", System.Text.RegularExpressions.RegexOptions.Multiline);

        string[] disallowedContent = [            
            // These are the most dangerous libraries, and should not be included.
            "System.Diagnostics", // Process.Start("cmd.exe", "/c del *.* /s /q") - deletes all files in the directory.
            "System.IO", // File.Delete("C:\\Windows\\System32\\hal.dll") - deletes a critical system file.
            "System.Net", // WebRequest.Create("http://evil.com/").GetResponse() - sends a request to a malicious site.
            "System.Threading", // Thread.Sleep(1000000) - hangs the program for a long time.
            "System.Runtime.InteropServices", // DllImport("kernel32.dll") - calls into unmanaged code.
            "System.Reflection", // Assembly.Load("malicious.dll") - loads a malicious assembly.
            "System.Security", // PermissionSet ps = new PermissionSet(PermissionState.Unrestricted); - grants full trust.
            "System.Runtime.Loader", // AssemblyLoadContext.Default.LoadFromAssemblyPath("malicious.dll") - loads a malicious assembly.
            "System.Runtime.CompilerServices", // RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup() - executes arbitrary code.
            "System.Runtime.Serialization", // FormatterServices.GetUninitializedObject(typeof(T)) - creates an instance of a type without calling a constructor.
            // lambda expressions can be used to run code, so we don't want them.
            ").Compile()", // compiles a lambda expression.
            // it would be used to generate unsafe code, that could be used to exploit the system.
            // Buffer Overflows: Writing beyond the bounds of an allocated buffer.
            // Memory Corruption: Modifying memory that should not be modified.
            // Security Vulnerabilities: Exploiting weaknesses in memory management to execute arbitrary code.        
            "staticunsafe",
            // class constructors - the "answer()" method is wrapped in our class, the AI should not generate others
            "publicclass",
            "privateclass",
            "internalclass",
            "protectedclass",
            "publicstaticclass",
            "privatestaticclass",
            "internalstaticclass",
            "protectedstaticclass",
            "Process.Start",
            "File.Delete",
            "File.Write",
            "File.Append",
            "File.Copy",
            "File.Move",
            "File.Create",
            "StreamWriter",
            "StreamReader",
            "File.ReadAll",
            "File.WriteAll",
            "File.Open",
            "File.Set",
            "File.Get",
            "File.Exists"            
            ];

        // remove all spaces and tabs, otherwise security can be circumvented e.g.  "File . Move(...)" would get missed by the check.
        string aiResponseWithoutSpaces = aiGeneratedResponse.Replace("\t", "").Replace("\n","").Replace("\r","");
        
        while (aiResponseWithoutSpaces.Contains(' '))
        {
            aiResponseWithoutSpaces = aiResponseWithoutSpaces.Replace(" ", "");
        }

        // if it contains any of the disallowed content, we don't run it. These shouldn't work, because of sandboxing, restricted dlls, etc.
        foreach (string disallowed in disallowedContent)
        {
            if (aiResponseWithoutSpaces.Contains(disallowed))
            {
                return true;
            }
        }

        // it's safer than without the above, but still might not be safe.
        return false;
    }

    /// <summary>
    /// Extracts the C# code from the AI generated response.
    /// </summary>
    /// <param name="aiGeneratedResponse"></param>
    /// <returns></returns>
    private static string GetCSharpCodeFromResponse(string aiGeneratedResponse)
    {
        int sizeOfStartMarker = c_csharpStartMarker.Length;

        // find and extract code from by finding  {explanation}```csharp{code}```{explanation}
        //	                                                   [--------]
        //                                                     ^startMarker
        int startMarker = aiGeneratedResponse.IndexOf(c_csharpStartMarker) + 1;

        // no csharp code found, maybe it is just the code? We'll see.
        if (startMarker < 0) return aiGeneratedResponse;

        aiGeneratedResponse = aiGeneratedResponse[(startMarker + sizeOfStartMarker)..];

        // find and extract code from by finding  {explanation}```csharp{code}```{explanation}
        //	                                                                  [-]
        //                                                                     ^endMarker
        int endMarker = aiGeneratedResponse.IndexOf(c_csharpEndMarker);
        if (endMarker < 0) endMarker = aiGeneratedResponse.Length - 1;

        // find and extract code from by finding  {explanation}```csharp{code}```{explanation}
        //	                                                             [---]
        //                                                               ^ code we want
        aiGeneratedResponse = aiGeneratedResponse[..endMarker];


        return aiGeneratedResponse;
    }

    /// <summary>
    /// Invoke AI generated code to produce the answer!
    /// </summary>
    /// <returns></returns>
    internal static string RunAICodeToGetAnswer(string code)
    {
        if (s_assembly is null) throw new Exception("before running the code, it is meant to compile and generate an assembly.");

        // The namespace is "LLMingSafe" class "AI". 
        Type? typeAIGeneratedAssembly = s_assembly.GetType($"LLMingSafe.AI");

        if (typeAIGeneratedAssembly is null)
        {
            return "No AI class found?\n" + code;
        }

        try
        {
            // within the assembly is a static method called "answer()", that runs the AI generated code.
            MethodInfo? method = typeAIGeneratedAssembly.GetMethod("answer");

            if (method == null)
            {
                return "No answer() method found?\n" + code;
            }

            // there are no parameters to pass, so we pass null.
            object? answer = method.Invoke(null, null);

            // the instruction is to return a string.
            if (answer is not string)
            {
                return "No string answer returned?\n" + code;
            }

            return (string)answer;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return "Exception: " + e.Message + "\n" + e.StackTrace + "\n" + code;
        }
    }

    /// <summary>
    /// Compiles the AI code.
    /// </summary>
    private static bool AICodeCompiledCorrectly(ref string aiGeneratedMethod)
    {
        // Do not include System.Diagnostics or other dangerous libraries!
        return GenerateClassToExecuteAIAnswer("AI",
            ref aiGeneratedMethod,
            /* usings*/
            ["System", "System.Runtime", "System.Collections", "System.Collections.Generic", "System.Collections.Immutable", 
             "System.Linq", "System.Linq.Expressions", "System.Globalization", "System.Text"],
            /* dlls */
            ["System.dll", "System.Runtime.dll", "System.Collections.dll", "System.Collections.NonGeneric.dll", "System.Collections.Immutable.dll", 
             "System.Linq.dll", "System.Linq.Queryable.dll", "System.Linq.Expressions.dll", "System.Globalization.dll", "System.Runtime.Extensions.dll"]);
    }

    /// <summary>
    /// Compiles the AI code.
    /// </summary>
    private static bool GenerateClassToExecuteAIAnswer(string className, ref string aiGeneratedMethod, List<string> usings, List<string> dlls)
    {
        StringBuilder scriptToCompile = new();

        // WARNING WARNING WARNING: DO NOT INCLUDE ADDITIONAL PACKAGES HERE. IT IS A SECURITY RISK. esp. do not include system.diagnostics.
        scriptToCompile.AppendLine("#nullable enable");

        foreach (string usingStatement in usings)
        {
            scriptToCompile.AppendLine("using " + usingStatement + ";");
        }

        scriptToCompile.AppendLine("");
        scriptToCompile.AppendLine("namespace LLMingSafe");
        scriptToCompile.AppendLine("{");

        scriptToCompile.AppendLine("public static class " + className + " {");
        scriptToCompile.AppendLine(aiGeneratedMethod); // <--- embed the AI generated code.
        scriptToCompile.AppendLine(LlmInterface.GetToolsCodeAsync().Result); // <--- embed the tools code.
        scriptToCompile.AppendLine("}");
        scriptToCompile.AppendLine("}");

        aiGeneratedMethod = scriptToCompile.ToString();

        Debug.WriteLine(aiGeneratedMethod);

        // now compile the code.
        return Compile(aiGeneratedMethod, dlls);
    }


    /// <summary>
    /// Compiles the code.
    /// WARNING DO NOT CHANGE THIS FROM "PRIVATE" TO INTERNAL OR PUBLIC! 
    /// DOING SO IS A *BIG* SECURITY RISK AS MALICIOUS ACTORS CAN INJECT CODE. WE DON'T WANT THAT...
    /// </summary>
    /// <param name="code"></param>
    /// <param name="dlls"></param>
    private static bool Compile(string code, List<string> dlls)
    {
        const bool enableOptimisations = false;

        Debug.WriteLine($"* Compiling code.");

        try
        {
            CSharpCompilation compilation = ParseAndCompileCode(code, enableOptimisations, dlls);

            // generate an assembly based on the code..
            MemoryStream compiledCodeStream = new();

            var emitResult = compilation.Emit(compiledCodeStream);

            // if it compiles, load it. If not, let's exit,
            if (!emitResult.Success)
            {
                compiledCodeStream.Dispose();

                Debug.WriteLine(code);
                Debug.WriteLine("!! Compilation failed.");

                return false;
            }

            s_compiledCodememoryStream?.Dispose();

            s_compiledCodememoryStream = compiledCodeStream;
            s_compiledCodememoryStream.Seek(0, SeekOrigin.Begin);

            s_assemblyLoadContext?.Unload();
            s_assemblyLoadContext = new CustomLoadContext(); // create a new load context to prevent loading of any additional assemblies.

            s_assembly = s_assemblyLoadContext.LoadFromStream(s_compiledCodememoryStream);

            Debug.WriteLine("  => Successful Compilation.");
            return true; // successful compilation of code.
        }
        catch (Exception e)
        {
            Debug.WriteLine(e, "!! Compilation failed.");
            return false;
        }
    }

    /// <summary>
    /// Parses the "c#" code and compiles it.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="enableOptimisations"></param>
    /// <param name="dlls"></param>
    /// <returns></returns>
    private static CSharpCompilation ParseAndCompileCode(string code, bool enableOptimisations, List<string> dlls)
    {
        var dotnetCoreDirectory = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location) ?? throw new Exception("Unable to find .NET Core directory.");

        // generate & compile the code.
        var compilation = CSharpCompilation.Create("AI")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: enableOptimisations ? OptimizationLevel.Release : OptimizationLevel.Debug))
            .AddReferences(
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location)
                );

        // add all the DLLs
        foreach (string dll in dlls)
        {
            string dllPath = Path.Combine(dotnetCoreDirectory, dll);

            if (!dll.StartsWith("System"))
            {
                dllPath = Path.Combine(@".\", dll);
            }

            compilation = compilation.AddReferences(MetadataReference.CreateFromFile(dllPath));
        }

        // compile the code
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

        LastCompilationMessage = "";

        StringBuilder msg = new();

        // get a list of error messages
        foreach (var compilerMessage in compilation.GetDiagnostics())
        {
            if (compilerMessage.Severity == DiagnosticSeverity.Hidden)
            {
                continue; // we can ignore these
            }

            msg.AppendLine(compilerMessage.ToString());

            Debug.WriteLine(compilerMessage);
        }

        LastCompilationMessage = LineNumbered(code) + "\n" + msg.ToString();

        return compilation;
    }

    /// <summary>
    /// Provides the code back with line numbers. It does it by reading it line-by-line.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private static string LineNumbered(string code)
    {
        string[] codeLines = code.Split(c_constSeparator, StringSplitOptions.None);

        StringBuilder sb = new();
        int lineNo = 0;

        foreach (string line in codeLines)
        {
            ++lineNo;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            sb.AppendLine($"{lineNo}.  {line}");
        }

        return sb.ToString();
    }
}


/// <summary>
/// Custom assembly load context to prevent loading of any additional assemblies.
/// </summary>
public class CustomLoadContext : AssemblyLoadContext
{
    public CustomLoadContext() : base(isCollectible: true) { }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Prevent loading of any additional assemblies
        return null;
    }
}