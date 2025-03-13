namespace LLMing.components;

public class ChatBrowser : WebBrowser
{
    /// <summary>
    /// The text to display when the AI is thinking
    /// </summary>
    const string c_thinkingHTML = "<div id='thinking'>Thinking...</div>";

    /// <summary>
    /// Stores the chat browser content (html).
    /// </summary>
    private string _chatBrowserContent = "";

    /// <summary>
    /// Template for the chat browser (styling etc).
    /// </summary>
    private static readonly string c_chatBrowserTemplate = File.ReadAllText(@".\web\template.html");

    /// <summary>
    /// Constructor.
    /// </summary>
    public ChatBrowser()
    {
        Dock = DockStyle.Fill;
        ScrollBarsEnabled = true;

        Navigating += WebBrowser_Navigating; // open hyperlinks in Chrome
    }

    /// <summary>
    /// Add text to the html (browser) content.
    /// Replaces newlines with <br> tags.
    /// Waits for the browser to render the content.
    /// </summary>
    /// <param name="htmlContent"></param>
    public void AddHTMLToChatBrowserContent(string htmlContent)
    {
        htmlContent = htmlContent.Replace("\n", "<br>");
        _chatBrowserContent += htmlContent;

        UpdateChatBrowserContent();
    }

    /// <summary>
    /// Remove HTML from the chat browser content.
    /// </summary>
    /// <param name="htmlToRemove"></param>
    public void RemoveHTML(string htmlToRemove)
    {
        // remove the html if present
        _chatBrowserContent = _chatBrowserContent.Replace(htmlToRemove, "");

        UpdateChatBrowserContent();
    }

    /// <summary>
    /// Update the chat browser content.
    /// </summary>
    private void UpdateChatBrowserContent()
    {
        string browserText = c_chatBrowserTemplate.Replace("{{content}}", _chatBrowserContent);
        DocumentText = browserText;

        Application.DoEvents();

        // wait for the browser to render the content
        while (ReadyState != WebBrowserReadyState.Complete)
        {
            Application.DoEvents();
        }

        try
        {
            Document?.Body?.All[Document.Body.All.Count - 1].ScrollIntoView(false);
        }
        catch
        {
            // sometimes the browser is not ready to scroll. We have to consume failure
        }
    }

    /// <summary>
    /// Clear the chat browser content.
    /// </summary>
    public void ClearChatBrowserContent()
    {
        _chatBrowserContent = "";
    }

    /// <summary>
    /// Open hyperlinks in Chrome.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal virtual void WebBrowser_Navigating(object? sender, WebBrowserNavigatingEventArgs? e)
    {
        if (e is null || e.Url is null) return; // we cannot click this


        // user clicked on "like", we store the result in the suggestions
        if (e.Url.ToString() == "about:like")
        {
            e.Cancel = true;

            return;
        }

        if (e.Url.Scheme != "http" && e.Url.Scheme != "https") return;

        // cancel the inbuilt navigation, and open the link in Chrome
        e.Cancel = true;

        Utils.OpenUrlInChrome(e.Url.ToString());
    }

    /// <summary>
    /// Display the thinking message.
    /// </summary>
    public void AddThinking()
    {
        AddHTMLToChatBrowserContent(c_thinkingHTML);
    }

    /// <summary>
    /// Remove the thinking message.
    /// </summary>
    public void RemoveThinking()
    {
        RemoveHTML(c_thinkingHTML);
    }
}