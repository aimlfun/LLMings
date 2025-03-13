using LLMing.components;
using LLMing.llm;
using LLMing.llm.suggest;
using LLMing.LLM;
using System.Diagnostics;

namespace LLMing;

/// <summary>
/// Form for calling the LLM (GPT-4-All) service.
/// </summary>
public partial class FormLLMing : Form
{
    /// <summary>
    /// Used to store the last question asked.
    /// </summary>
    private string _lastQuestion = "";

    /// <summary>
    /// Tracks the question and answer for the LIKE buttons to work correctly.
    /// </summary>
    private readonly Dictionary<int, (string Question, string Answer)> _cachedQandAforLike = [];

    /// <summary>
    /// Constructor
    /// </summary>
    public FormLLMing()
    {
        InitializeComponent();

        LoadListOfAvailableModels();
    }

    /// <summary>
    /// Call the LLM service to get the models available.
    /// </summary>
    private void LoadListOfAvailableModels()
    {
        comboBoxModels.Items.Clear();

        List<string>? models = Gpt4AllServiceWrapper.GetModels();

        // no models returned (error or none available), onload will close the form
        if (models is null || models.Count == 0) return;

        // populate the combo box with the models
        foreach (string model in models)
        {
            comboBoxModels.Items.Add(model);
        }

        comboBoxModels.SelectedValueChanged += ComboBoxModelsChangedSetLLMModel; // the LLM needs to know which model to use

        comboBoxModels.SelectedIndex = 0;
    }

    /// <summary>
    /// Sets the model to use.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBoxModelsChangedSetLLMModel(object? sender, EventArgs e)
    {
        if (comboBoxModels.SelectedItem is not string model) return;

        LlmInterface.SetModel(model);
    }

    /// <summary>
    /// When the form loads, it confirms the LLM service is running.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Form1_Load(object sender, EventArgs e)
    {
        if (comboBoxModels.Items.Count == 0)
        {
            // Check the GPT4All exe is running. If it is, then go to settings and ensure you've anebled the API server.
            // Settings > Enable Local API Server [x]
            // If it *is* running, did you install a model? Pick "Reasoner v1".
            MessageBox.Show("No models found. The Nomic GPT4All LLM REST API service does not appear to be running. Please start GPT4All, and ensure API is enabled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        ResponseBrowser.OnLike += ResponseBrowser_OnLike;

        _chatWebBrowser.ClearChatBrowserContent();
        _chatWebBrowser.AddHTMLToChatBrowserContent("<b>What would you like to know?</b>\n\n");

        _responseBrowser.ClearChatBrowserContent();

        textBoxQuestionToAskAI.Focus();
    }

    /// <summary>
    /// When LIKE is clicked, it stores the question and answer as a tool suggestion.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResponseBrowser_OnLike(object? likeNumber, EventArgs e)
    {
        // if the like number is not a string, we can't use it. We shouldn't be called if it's not a string, but just in case.
        if (likeNumber is not string likeIndexString) return;

        // if the like number is not a number, we can't use it.
        if (!int.TryParse(likeIndexString, out int likeIndex)) return;

        // if the like number is not in the cache, we can't use it.
        if (!_cachedQandAforLike.TryGetValue(likeIndex, out (string Question, string Answer) questionAnswer)) return;

        // now make an "example" from the question and answer, and add it to the tool suggestions.

        (string Question, string Answer) = questionAnswer;

        Example toolSuggestion = Example.CreateExample(Question, Answer);

        LlmInterface.AddToolSuggestion(toolSuggestion);

        textBoxQuestionToAskAI.Focus(); // try to maintain focus on the question box
    }

    /// <summary>
    /// Asks the AI to answer the question.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ButtonAskAI_Click(object sender, EventArgs e)
    {
        // if no question, don't ask
        string question = textBoxQuestionToAskAI.Text.Trim();

        if (string.IsNullOrWhiteSpace(question)) return; // don't ask LLM "nothing"

        _lastQuestion = question;

        // disable the button whilst we answer.
        buttonAskAI.Enabled = false; // user can't request again until it completes, but they can edit the question

        textBoxQuestionToAskAI.Text = ""; // clear the question, ready for the next one

        // feedback to the user that we are thinking, and what the AI is instructed to do.
        await DisplayQuestionOnBothSidesAndInstructions(question);

        try
        {
            Stopwatch stopwatchForLLMResponse = new();
            stopwatchForLLMResponse.Start();

            // ask the LLM the question
            (string Answer, string HtmlResponse, bool IsSuccessful) response = await Task.Run(() => LlmInterface.AnswerQuestionAsync(question));

            stopwatchForLLMResponse.Stop();

            // output the response on both panels
            OutputLlmResponse(response, stopwatchForLLMResponse.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _responseBrowser.AddHTMLToChatBrowserContent(ex.Message + "\n<hr>");
        }

        // use could change the question, but not ask another whilst we're still answering.
        buttonAskAI.Enabled = true;

        textBoxQuestionToAskAI.Focus();
    }

    /// <summary>
    /// Repeats the question on both sides.
    /// For the right side, it will output the instructions to the AI, and any prior examples
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    private async Task DisplayQuestionOnBothSidesAndInstructions(string question)
    {
        // question on left, followed by thinking dots
        _chatWebBrowser.AddHTMLToChatBrowserContent("<b>" + question + "</b>\n");
        _chatWebBrowser.AddThinking();

        // show the instructions that will be given to the AI on the right
        string instructions = await LlmInterface.GetInstructionsAsync(question); // THIS IS INEFFICIENT, BECAUSE THE CALL TO AI WILL ALSO DO THIS. BUT IT HELPS THE USER.

        if (!string.IsNullOrWhiteSpace(instructions)) instructions = "<code>" + instructions + "</code>\n\n";

        _responseBrowser.AddHTMLToChatBrowserContent("<b>" + question + "</b>\n" + (checkBoxOutputTranscript.Checked ? instructions : ""));

        // ensure the screen paints as we've added content
        Application.DoEvents();
    }

    /// <summary>
    /// Outputs the AI response/answer to question.
    /// </summary>
    /// <param name="response"></param>
    private void OutputLlmResponse((string Answer, string HtmlResponse, bool IsSuccessful) response, long responseTime)
    {
        _chatWebBrowser.RemoveThinking(); // remove the thinking dots (we add them when we're waiting for a response)

        _chatWebBrowser.AddHTMLToChatBrowserContent(response.HtmlResponse + "\n\n");

        string aiResponse = $"<b>Response:</b> <span class='elapsed'>{responseTime}ms</span>\n" +
            $"\n" +
            $"{Utils.BeautifyCodeResponse(response.Answer)}";

        // if the response worked, add the like button. This is used to store the suggestion.
        // Future suggestions are used to improve the AI's responses, by giving it a hint.
        if (response.IsSuccessful)
        {
            int likeIndex = _cachedQandAforLike.Count;
            _cachedQandAforLike.Add(likeIndex, (_lastQuestion, CodeGenerator.LastAnswerFunction));

            aiResponse +=
                "\n\n" +
                $"If this is a good answer, click <a title='Use this answer for similar future questions.' href='like{likeIndex}'>LIKE</a>.";
        }

        _responseBrowser.AddHTMLToChatBrowserContent(aiResponse + "<hr>");
    }

    /// <summary>
    /// [Guidance] button clicked. Shows the dialog to edit the guidance.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonGuidance_Click(object sender, EventArgs e)
    {
        // call Visual Studio's code editor to edit the guidance
        Utils.OpenFileUsingVisualCodeOrStudio(LlmInterface.GetGuidanceFilePath());
    }

    /// <summary>
    /// [Tools] button clicked. Shows the dialog to edit the "tools.cs".
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonTools_Click(object sender, EventArgs e)
    {
        // call Visual Studio's code editor to edit the tools
        Utils.OpenFileUsingVisualCodeOrStudio(LlmInterface.GetToolsFilePath());
    }

    /// <summary>
    /// [Examples] button clicked. Shows the dialog to edit the "tool-suggestions.json".
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonExamples_Click(object sender, EventArgs e)
    {
        Utils.OpenFileUsingVisualCodeOrStudio(ExampleManager.GetToolSuggestions());
        MessageBox.Show("Please restart to apply any external changes made, to avoid losing the change.", "Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}