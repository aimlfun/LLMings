namespace LLMing.llm.gpt4all;

/// <summary>
/// 
/// </summary>
internal class Gpt4AllResponse
{
    public List<Gpt4AllChoice> Choices { get; set; }
    public long created { get; set; }
    public string? id { get; set; }
    public string? model { get; set; }
    public string? Object { get; set; }
    public Gpt4AllUsage? usage { get; set; }
}