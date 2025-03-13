namespace LLMing.llm.suggest;

/// <summary>
/// Represents a suggestion for a tool that could be used to solve a problem.
/// </summary>
/// <param name="inferenceQuestion"></param>
/// <param name="userQuestion"></param>
/// <param name="answer"></param>
public class Example(string inferenceQuestion, string userQuestion, string answer)
{
    /// <summary>
    /// The question in inferable form.
    /// </summary>
    public string InferenceQuestion { get; set; } = inferenceQuestion;

    /// <summary>
    /// The question asked by the user.
    /// </summary>
    public string UserQuestion { get; set; } = userQuestion;

    /// <summary>
    /// Example answer to the question (code form).
    /// </summary>
    public string Answer { get; set; } = answer;

    /// <summary>
    /// Constructor. Used for creating new ones.
    /// </summary>
    /// <param name="question">Question AI was asked by User.</param>
    /// <param name="answer">Answer output by AI (code).</param>
    public static Example CreateExample(string question, string answer)
    {
        // remove any newlines and double spaces, and the question mark; they serve no purpose.
        question = question.Replace("?", "").Replace("\n", "").Replace("  ", " ");
        answer = answer.Replace("  ", " ").Replace("  ", " ");

        string inferenceQuestion = question.ToLower();

        // when we change *'s -> *. The apostrophe-s is surplus.
        inferenceQuestion = inferenceQuestion.Replace("*'s ", "* ");

        return new Example(inferenceQuestion, question, answer);
    }
}