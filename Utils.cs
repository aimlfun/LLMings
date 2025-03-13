using Microsoft.Win32;
using System.Diagnostics;

namespace LLMing;

/// <summary>
/// Utility functions.
/// </summary>
internal static class Utils
{
    /// <summary>
    /// Cache the location to avoid 
    /// </summary>
    private static string? s_visualStudioLocation = null;

    /// <summary>
    /// Used to find code chunks in the response, so they can be styled as code.
    /// </summary>
    /// ○ Match '`'.<br/>
    /// ○ 1st capture group.<br/>
    ///     ○ Match a character other than '\n' lazily any number of times.<br/>
    /// ○ Match '`'.<br/>
    private static readonly System.Text.RegularExpressions.Regex CodeChunksWrapped = new("`(.*?)`");

    /// <summary>
    /// Open a URL in Chrome.
    /// </summary>
    /// <param name="url"></param>
    internal static void OpenUrlInChrome(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        if (!url.StartsWith("http://") && url.StartsWith("https://"))
        {
            return;
        }

        using Process process = new();
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = "chrome";
        process.StartInfo.Arguments = url;
        process.Start();
    }

    /// <summary>
    /// Return the response styled as HTML. This includes code blocks and variables.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    internal static string BeautifyCodeResponse(string response)
    {
        // we're returning HTML. The response is plain text.

        // these won't render as html thinks they are tags, so we need to escape them.
        // It's a bit of a hacky html-encode, but suffices for this test app.

        response = response.Replace("&", "&amp;");

        response = response.Replace("<", "&lt;");
        response = response.Replace(">", "&gt;");

        // anything between "```csharp"  and "```" is C# code and should be styled as such.
        response = response.Replace("```csharp", "<pre><code>").Replace("```", "</code></pre>");

        // anything between "`" and "`" is a variable and should be styled as such.
        // `some text` -> <code>some text</code> using regex.
        response = CodeChunksWrapped.Replace(response, "<code>$1</code>");

        response = response.Replace("\t", "  "); // tabs render in <code/> but are a bit big, so we make it a couple of spaces

        return response;
    }

    /// <summary>
    /// Get the path to Visual Studio Code.
    /// </summary>
    /// <returns>null if not foud</returns>
    internal static string? GetVSCodePath()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\Code.exe\shell\open\command");

        if (key is null) return null; // not installed

        object? value = key.GetValue(null);

        if (value is null) return null; // not installed

        string vsCodePath = string.Empty;
        vsCodePath = ((string)value).ToString().Trim('"');
        vsCodePath = vsCodePath[..(vsCodePath.LastIndexOf("Code.exe") + "Code.exe".Length)]; // excludes "%1"

        if (!File.Exists(vsCodePath))
        {
            return null;
        }

        s_visualStudioLocation = vsCodePath;

        return string.IsNullOrEmpty(vsCodePath) ? null : vsCodePath;
    }

    /// <summary>
    /// Get the path to Visual Studio 2022.
    /// </summary>
    /// <returns>null if not found</returns>
    internal static string? GetVisualStudioPath()
    {
        try
        {
            // Path to vswhere.exe
            string? vswherePath = GetVsWherePath();

            if (!File.Exists(vswherePath))
            {
                return null;
            }

            // Use vswhere to locate Visual Studio 2022
            ProcessStartInfo startInfo = new()
            {
                FileName = vswherePath,
                Arguments = "-latest -requires Microsoft.Component.MSBuild -find Common7\\IDE\\devenv.exe",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using Process? process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            using StreamReader reader = process.StandardOutput;
            string result = reader.ReadToEnd().Trim();

            if (!string.IsNullOrEmpty(result))
            {
                s_visualStudioLocation = result;
                return result; // Return the path to devenv.exe
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while locating Visual Studio: {ex.Message}");
        }

        return null; // Visual Studio not found
    }

    /// <summary>
    /// Get the path to vswhere.exe.
    /// </summary>
    /// <returns></returns>
    static string? GetVsWherePath()
    {
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string possiblePath = Path.Combine(programFiles, "Microsoft Visual Studio", "Installer", "vswhere.exe");

        if (File.Exists(possiblePath))
        {
            return possiblePath;
        }

        // Check Program Files if not found in Program Files (x86)
        programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        possiblePath = Path.Combine(programFiles, "Microsoft Visual Studio", "Installer", "vswhere.exe");

        if (File.Exists(possiblePath))
        {
            return possiblePath;
        }

        return null;
    }

    /// <summary>
    /// Opens a file using Visual Studio Code, or fallback of Visual Studio.
    /// </summary>
    /// <param name="filePath"></param>
    internal static void OpenFileUsingVisualCodeOrStudio(string filePath)
    {        
        // Path to Visual Studio Code executable
        string? vsCodePath = s_visualStudioLocation  ?? Utils.GetVSCodePath() ?? Utils.GetVisualStudioPath();

        if (vsCodePath is null)
        {
            // we use them as editors
            MessageBox.Show($"Visual Studio Code and Visual Studio were not found. Please install one of them.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!filePath.StartsWith('\"')) filePath = $"\"{filePath}\"";

        // Start Visual Studio / Code with the file to enable user to edit it.
        Process.Start(vsCodePath, filePath);
    }
}