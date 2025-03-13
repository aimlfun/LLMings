using Newtonsoft.Json;
using LLMing.llm.gpt4all;

namespace LLMing.LLM;

/// <summary>
/// Wrapper for the GPT-4-All REST API service.
/// </summary>
internal static class Gpt4AllServiceWrapper
{
    /// <summary>
    /// Endpoint for the LLM chat completions.
    /// </summary>
    private const string c_llmEndPoint = "http://localhost:4891/v1/chat/completions";

    /// <summary>
    /// Endpoint for the models available in the LLM.
    /// </summary>
    private const string c_modelsEndPoint = "http://localhost:4891/v1/models";

    /// <summary>
    /// Client to make HTTP requests.
    /// </summary>
    private static readonly HttpClient s_client = new()
    {
        Timeout = TimeSpan.FromMinutes(20) // Set the timeout to X minutes
    };

    /// <summary>
    /// Returns the list of models available in the LLM.
    /// </summary>
    /// <returns></returns>
    public static List<string>? GetModels()
    {
        List<string> models = [];

        try
        {
            string url = c_modelsEndPoint;

            var response = s_client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string result = response.Content.ReadAsStringAsync().Result;
            
            dynamic? data = JsonConvert.DeserializeObject(result) ?? throw new Exception("No data returned from the model.");
            
            foreach (var item in data.data)
            {
                models.Add(item.id.ToString());
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }

        return models;
    }

    /// <summary>
    /// Returns the details of an LLM.
    /// </summary>
    /// <param name="modelName"></param>
    /// <returns></returns>
    public static string? GetModelDetails(string modelName)
    {
        string modelDetails;

        try
        {
            string url = $"http://localhost:4891/v1/models/{modelName}";

            var response = s_client.GetAsync(url).Result;

            response.EnsureSuccessStatusCode();
            modelDetails = response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }

        return modelDetails;
    }

    /// <summary>
    /// Ask the model a question and get a completion (answer).
    /// </summary>
    /// <param name="modelName">The name of the model.</param>
    /// <param name="modelGuidance">Used as a "system" prompt to guide the model.</param>
    /// <param name="userQuestion">The question being asked.</param>
    /// <param name="maxTokens">The max number of tokens.</param>
    /// <param name="temperature">0=no hallucination up to 1=severly flaky</param>
    /// <returns></returns>
    public async static Task<Gpt4AllResponse> GenerateChatCompletions(string modelName, string modelGuidance, string userQuestion, int maxTokens, double temperature)
    {
        userQuestion = MinifyLlmInput(userQuestion);
        modelGuidance = MinifyLlmInput(modelGuidance);

        Gpt4AllResponse? completionsObject;

        try
        {
            string url = c_llmEndPoint; // API endpoint for chat completions

            List<dynamic> allMessages = [];

            // instructions to the model
            allMessages.Add(new
            {
                role = "system",
                content = modelGuidance, // guidance for the model (instructions what it should do)
            });

            // add the question (from user)
            allMessages.Add(new
            {
                role = "user",
                content = userQuestion, // what the user asked
            });


            // wrap up instructions, and questions
            var json = JsonConvert.SerializeObject(new
            {
                model = modelName,
                messages = allMessages,
                max_tokens = maxTokens,
                temperature
            });

            // send the LLM request
            StringContent content = new(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = s_client.PostAsync(url, content).Result;

            response.EnsureSuccessStatusCode();
            string completions = await response.Content.ReadAsStringAsync();

            completionsObject = JsonConvert.DeserializeObject<Gpt4AllResponse>(completions);

            if (completionsObject is null)
            { 
                throw new Exception("No response from the model.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

        return completionsObject;
    }

    /// <summary>
    /// Minify the input for the LLM, reducing the text without loss of fidelity.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string MinifyLlmInput(string input)
    {
        input = input.Replace("\r", "");  // remove carriage returns
        input = input.Replace("\n\n", "\n");  // remove extra new lines
        input = input.Replace("\t", " "); // replace tabs
        input = input.Replace("  ", " ").Replace("  ", " ").Replace("  ", " "); // remove extra spaces
        input = input.Trim(); // remove leading and trailing spaces

        return input;
    }
}