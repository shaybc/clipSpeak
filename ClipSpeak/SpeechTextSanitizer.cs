using System.Net;
using System.Text.RegularExpressions;

namespace ClipSpeak;

internal static class SpeechTextSanitizer
{
    private const string UrlReplacement = "HTTP URL";

    private static readonly Regex HtmlAnchorRegex = new(
        @"<a\b[^>]*\bhref\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)[^>]*>(?<text>.*?)</a>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex MarkdownLinkRegex = new(
        @"(?<!!)\[(?<text>[^\]\r\n]*)\]\(\s*(?:(?:https?://|www\.|file:///|[A-Za-z]:[\\/]|\\\\|/)[^)]*)(?:\s+""[^""]*"")?\s*\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MarkdownAutolinkRegex = new(
        @"<\s*(?:https?://|www\.)[^\s>]+\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FileUriRegex = new(
        @"<?\bfile:///(?<path>[^\s<>""']*[\\/](?<file>[^\\/\s<>""']+\.[A-Za-z0-9]{1,12}))>?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FilePathRegex = new(
        @"(?<![\w:/\\])(?:[A-Za-z]:|\\\\[^\\/\s]+[\\/][^\\/\s]+|/)[^\r\n<>""|]*[\\/](?<file>[^\\/\r\n<>:""|?*]+\.[A-Za-z0-9]{1,12})",
        RegexOptions.Compiled);

    private static readonly Regex BareUrlRegex = new(
        @"\b(?:https?://|www\.)[^\s<>()]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HtmlTagRegex = new(
        @"<[^>]+>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    public static string SuppressUrls(string text)
    {
        var sanitizedText = HtmlAnchorRegex.Replace(text, match => LinkTextOrUrlReplacement(match.Groups["text"].Value));
        sanitizedText = MarkdownLinkRegex.Replace(sanitizedText, match => LinkTextOrUrlReplacement(match.Groups["text"].Value));
        sanitizedText = MarkdownAutolinkRegex.Replace(sanitizedText, UrlReplacement);
        sanitizedText = BareUrlRegex.Replace(sanitizedText, ReplaceBareUrl);
        sanitizedText = FileUriRegex.Replace(sanitizedText, ReplaceFilePath);
        return FilePathRegex.Replace(sanitizedText, ReplaceFilePath);
    }

    private static string LinkTextOrUrlReplacement(string linkText)
    {
        var visibleText = HtmlTagRegex.Replace(linkText, string.Empty);
        visibleText = WebUtility.HtmlDecode(visibleText).Trim();
        return string.IsNullOrWhiteSpace(visibleText) ? UrlReplacement : visibleText;
    }

    private static string ReplaceFilePath(Match match)
    {
        return TrimTrailingPunctuation(match.Groups["file"].Value);
    }

    private static string ReplaceBareUrl(Match match)
    {
        return UrlReplacement + GetTrailingPunctuation(match.Value);
    }

    private static string TrimTrailingPunctuation(string text)
    {
        var trimmedText = text;
        while (trimmedText.Length > 0 && IsTrailingPunctuation(trimmedText[^1]))
        {
            trimmedText = trimmedText[..^1];
        }

        return trimmedText;
    }

    private static string GetTrailingPunctuation(string text)
    {
        var trailingPunctuation = string.Empty;
        while (text.Length > 0 && IsTrailingPunctuation(text[^1]))
        {
            trailingPunctuation = text[^1] + trailingPunctuation;
            text = text[..^1];
        }

        return trailingPunctuation;
    }

    private static bool IsTrailingPunctuation(char value)
    {
        return value is '.' or ',' or ';' or ':' or '!' or '?';
    }
}