namespace ClipSpeak;

internal sealed class ClipboardReader
{
    public string? TryGetText()
    {
        try
        {
            if (!Clipboard.ContainsText())
            {
                return null;
            }

            var text = Clipboard.GetText(TextDataFormat.UnicodeText);
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (Exception ex) when (ex is ExternalException || ex is ThreadStateException)
        {
            return null;
        }
    }
}
