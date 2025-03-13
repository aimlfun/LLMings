using System.Text.Json;

namespace LLMing.llm.suggest;

/// <summary>
/// Manages a list of examples with snippets and suggestions.
/// </summary>
internal class ExampleManager
{    
    /// <summary>
    /// Fetches suggestions.
    /// </summary>
    private static readonly ExampleManager s_toolSuggestionsManager = new(@".\llm\tool-suggestions.json");

    /// <summary>
    /// Writes the JSON with indentation, so it's easier to read.
    /// </summary>
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    /// <summary>
    /// The list of examples.
    /// </summary>
    private List<Example> _examples;

    /// <summary>
    /// The file path where the tool suggestions are stored.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="filePath"></param>
    internal ExampleManager(string filePath)
    {
        _filePath = filePath;
        _examples = [];

        Load();        
    }

    /// <summary>
    /// Returns the file path.
    /// </summary>
    /// <returns></returns>
    internal static string GetToolSuggestions()
    {
        return s_toolSuggestionsManager._filePath;
    }

    /// <summary>
    /// Adds an example to the list.
    /// </summary>
    /// <param name="example"></param>
    internal void Add(Example example)
    {
        _examples.Add(example);
        Save();
    }

    /// <summary>
    /// Removes an example from the list.
    /// </summary>
    /// <param name="example"></param>
    internal void Remove(Example example)
    {
        _examples.Remove(example);
        Save();
    }

    /// <summary>
    /// Loads the examples from the file.
    /// </summary>
    internal void Load()
    {
        _examples = [];

        if (!File.Exists(_filePath)) return; // no file, no load

        try
        {
            string json = File.ReadAllText(_filePath);
            _examples = JsonSerializer.Deserialize<List<Example>>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading tool suggestions: {e.Message}");
        }

    }

    /// <summary>
    /// Saves the examples to the file.
    /// </summary>
    internal void Save()
    {
        string json = JsonSerializer.Serialize(_examples, options: s_jsonSerializerOptions);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Returns all the examples.
    /// </summary>
    /// <returns></returns>
    internal static List<Example> GetAllExamples()
    {
        return s_toolSuggestionsManager._examples;
    }

    /// <summary>
    /// Adds a suggestion to the list.
    /// </summary>
    /// <param name="suggestion"></param>
    internal static void AddSuggestion(Example suggestion)
    {
        s_toolSuggestionsManager._examples.Add(suggestion);
        s_toolSuggestionsManager.Save();
    }
}
