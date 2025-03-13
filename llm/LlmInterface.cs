using System.Diagnostics;
using System.Text;
using LLMing.llm.gpt4all;
using LLMing.llm.suggest;
using LLMing.llm;

namespace LLMing.LLM;

/// <summary>
/// The interface to the LLM, includes the ability to compile and execute code.
/// </summary>
internal static class LlmInterface
{
    /// <summary>
    /// The name of the model used by the LLM.
    /// </summary>
    private static string s_llmModelName = "Reasoner v1"; // "DeepSeek-R1-Distill-Qwen-7B" was poor

    /// <summary>
    /// Assigns the model to use.
    /// </summary>
    /// <param name="modelName"></param>
    internal static void SetModel(string modelName)
    {
        s_llmModelName = modelName;
    }

    /// <summary>
    /// Returns the path to the guidance file.
    /// This file contains the instructions for the AI.
    /// </summary>
    /// <returns></returns>
    internal static string GetGuidanceFilePath()
    {
        // one file per model, so they can be different
        string filepath = $@".\llm\{s_llmModelName}\core.txt";

        if (!File.Exists(filepath))
        {
            string defaultGuidance = File.ReadAllText($@".\llm\core.txt");

            File.WriteAllText(filepath, defaultGuidance); // create a default.
        }

        return filepath;
    }

    /// <summary>
    /// Returns the path to the tools file.
    /// This file contains the tools that the AI can use.
    /// </summary>
    /// <returns></returns>
    internal static string GetToolsFilePath()
    {
        string toolsFilePath = $@".\llm\{s_llmModelName}\tools.cs";

        if (!File.Exists(toolsFilePath))
        {
            string defaultToolscs = File.ReadAllText($@".\llm\tools.cs.txt");

            File.WriteAllText(toolsFilePath, defaultToolscs); // create a default
        }

        return toolsFilePath;
    }

    /// <summary>
    /// Read the core file.
    /// </summary>
    /// <returns></returns>
    internal static async Task<string> GetGuidanceAsync()
    {
        EnsureDirectoryExists();

        string coreFilePath = GetGuidanceFilePath();
        
        return await File.ReadAllTextAsync(coreFilePath);
    }

    /// <summary>
    /// Read tools.
    /// </summary>
    /// <returns></returns>
    internal static async Task<string> GetToolsCodeAsync()
    {
        EnsureDirectoryExists();

        string toolsFilePath = GetToolsFilePath();

        string code = await File.ReadAllTextAsync(toolsFilePath);
        
        return code;
    }

    /// <summary>
    /// Create the directory if it doesn't exist.
    /// </summary>
    private static void EnsureDirectoryExists()
    {
        string toolsPath = $@".\llm\{s_llmModelName}";

        if (!Directory.Exists(toolsPath))
        {
            Directory.CreateDirectory(toolsPath);
        }
    }

    /// <summary>
    /// Adds a good suggestion (Like) to the tool suggestions.
    /// </summary>
    /// <param name="suggestion"></param>
    internal static void AddToolSuggestion(Example suggestion)
    {
        ExampleManager.AddSuggestion(suggestion);
    }

    /// <summary>
    /// Use an LLM to answer the users' question, by applying guidance/instructions in front of the question and requesting an answer as c# code.
    /// Hopefully it returns the code, we compile and execute it.
    /// 
    /// To SandBox the code, we use a separate assembly load context, with limited "using"s. Be careful if changing it. 
    /// Do not include System.Diagnostics, File.IO or another security risk. You don't want the AI going rogue and wiping your hard drive.
    /// </summary>
    /// <param name="question"></param>
    /// <returns>Tuple with answer, response and whether it was successful.</returns>
    internal async static Task<(string Answer, string HtmlResponse, bool IsSuccessful)> AnswerQuestionAsync(string question)
    {
        // read the instructions, examples and tools from the LLM folder.

        string instructions = await GetInstructionsAsync(question);

        Gpt4AllResponse? response = await Gpt4AllServiceWrapper.GenerateChatCompletions(
            modelName: s_llmModelName, /* the model to use */
            modelGuidance: instructions, /* what the AI should do */
            userQuestion: question, /* what the user asked */
            maxTokens: 16384, /* 131072 is what reasoner supports */
            temperature: 0.0 /* don't make stuff up! */);

        string answer = response.Choices[0].Message.Content;

        // if the code is a csharp code block, compile and execute
        string result = CodeGenerator.CompileAndExecute(answer, out bool codeRuns);

        if (!codeRuns)
        {
            // if it failed to return code, they can see what happened. If it succeeded, the answer is replaced with the result.
            string theResult = result;
            answer = result + "\n" + answer;
            result = theResult;
        }
     
        response.Choices[0].Message.Answer = result;
        response.Choices[0].Message.Content = answer;

        // if it failed to return code, they can see what happened. If it succeeded, the answer is replaced with the result.
        return (Answer: answer, HtmlResponse: result, IsSuccessful: codeRuns);
    }

    /// <summary>
    /// Get the instructions from the LLM.
    /// </summary>
    /// <returns></returns>
    internal static async Task<string> GetInstructionsAsync(string question)
    {
        question = question.Replace("\n", "");

        // the tools.txt file contains the tools that the AI can use.
        string tools = await GetToolsCodeAsync();

        // the functions are extracted from the tools, and added to the instructions.
        string functions = 
            "Functions: [\n" +
            $"{GetFunctionsFromTools(tools, out string toolsMinimised)}\n" +
            "]\n";

        // add the functions to the instructions.
        string LLM = functions + await GetGuidanceAsync();

        string instructions = LLM.Replace("{{Tools}}", toolsMinimised);

        instructions = InjectSimilarQuestionsWithAnswersAsExamples(question, instructions);

        return instructions;
    }

    /// <summary>
    /// Get the functions from the tools.
    /// We expect the lines to begin "// Use '...", and when found we extract the function with usage instructions.
    /// </summary>
    /// <param name="tools"></param>
    /// <returns></returns>
    private static string GetFunctionsFromTools(string tools, out string toolsMinimised)
    {
        string[] linesOfTools = tools.Split('\n');

        List<string> functions = [];
        StringBuilder minimised = new(1000);

        for (int lineNumber = 0; lineNumber < linesOfTools.Length; lineNumber++)
        {
            string line = linesOfTools[lineNumber];

            if (line.StartsWith("//"))                
            {
                if (line.Contains("Use")) // Use is a keyword to start the functions.)
                {
                    string function = line.Replace("///", "").Replace("//", "").Trim();

                    // function will be one of these 
                    // - "// Use 'int GetTanksDestroyedIn3dayWar(string country)' to answer how many tanks belonging to country were destroyed in Ukraine." <<- contains function
                    // - "// Use to answer how many tanks belonging to country were destroyed in Ukraine." <<- requires us to extract function

                    // if function is item 2, we need to read the next line to

                    if (!function.StartsWith("Use '"))
                    {
                        // read the function.
                        string functionDeclaration = linesOfTools[lineNumber + 1].Trim();

                        if (functionDeclaration.StartsWith("private")) functionDeclaration = functionDeclaration[7..].Trim();
                        if (functionDeclaration.StartsWith("public")) functionDeclaration = functionDeclaration[6..].Trim();
                        if (functionDeclaration.StartsWith("internal")) functionDeclaration = functionDeclaration[7..].Trim(); 
                        
                        // we don't bother with protected.

                        if (functionDeclaration.StartsWith("static")) functionDeclaration = functionDeclaration[6..].Trim();

                        function = "Use '" + functionDeclaration + "' " + function[4..].Trim();
                    }

                    functions.Add(function);
                }
            }
            else
            {
                minimised.AppendLine(line);
            }
        }

        toolsMinimised = minimised.ToString();

        return string.Join("\n", functions);
    }

    /// <summary>
    /// Give the AI answers to similar questions, as examples (where we have them).
    /// </summary>
    /// <param name="question"></param>
    /// <param name="instructions"></param>
    /// <returns></returns>
    private static string InjectSimilarQuestionsWithAnswersAsExamples(string question, string instructions)
    {
        List<Example> toolSuggestions = ExampleManager.GetAllExamples();
        Dictionary<float, List<Example>> matches = [];

        foreach (Example suggestion in toolSuggestions)
        {
            float match = InferenceMatcher.Match(suggestion.InferenceQuestion, question);

            Debug.WriteLine($"{match} <= {suggestion.InferenceQuestion}");

            if (match > 0.80) // 80% match
            {
                if (!matches.TryGetValue(match, out List<Example>? examples))
                {
                    examples = [];
                    matches.Add(match, examples);
                }

                examples.Add(suggestion);
            }
        }

        // if we have a match, we can add the examples (question+answer) to the instructions.
        if (matches.Count > 0)
        {
            instructions += "\nEXAMPLE QUESTION AND ANSWERS:\n";
            
            float bestMatch = matches.OrderByDescending(x => x.Key).First().Key;
            
            foreach (Example suggestion in matches[bestMatch])
            {
                instructions += $"When user asks \"{suggestion.UserQuestion}\"\n you answer \n{suggestion.Answer}\n";
            }
        }

        return instructions;
    }
}

