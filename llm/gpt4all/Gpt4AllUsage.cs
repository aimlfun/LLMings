namespace LLMing.llm.gpt4all;

/// <summary>
/// 
/// </summary>
internal class Gpt4AllUsage
{
    public int completion_tokens { get; set; }
    public int prompt_tokens { get; set; }
    public int total_tokens { get; set; }
}