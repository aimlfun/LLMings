namespace LLMing.components;

public class ResponseBrowser : ChatBrowser
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public ResponseBrowser()
    {
    }

    /// <summary>
    /// When the user clicks on "like", we store the result in the suggestions.
    /// </summary>
    public static event EventHandler? OnLike;

    /// <summary>
    /// When "like" is clicked, it stores the question and answer as a tool suggestion.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal override void WebBrowser_Navigating(object? sender, WebBrowserNavigatingEventArgs? e)
    {
        if (e is null || e.Url is null) return; // we cannot click this

        // user clicked on "like", we store the result in the suggestions
        if (e.Url.ToString().StartsWith("about:like"))
        {
            string likeNumber = e.Url.ToString()[10..];
            e.Cancel = true;

            OnLike?.Invoke(likeNumber, EventArgs.Empty);
        }
    }
}