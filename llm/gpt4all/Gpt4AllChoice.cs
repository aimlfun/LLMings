namespace LLMing.llm.gpt4all;

/// <summary>
/// 
/// </summary>
internal class Gpt4AllChoice
{
    public string finish_reason { get; set; }
    public int index { get; set; }
    public object Logprobs { get; set; }
    public Gpt4AllMessage Message { get; set; }
    public object References { get; set; }
}
