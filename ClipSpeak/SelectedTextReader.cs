using System.Windows.Automation;

namespace ClipSpeak;

internal sealed class SelectedTextReader
{
    public string? TryGetSelectedText(IntPtr targetWindow = default)
    {
        try
        {
            ForegroundWindow.TryActivate(targetWindow);

            var focusedElement = AutomationElement.FocusedElement;
            if (focusedElement is null ||
                !focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) ||
                pattern is not TextPattern textPattern)
            {
                return null;
            }

            var selectedRanges = textPattern.GetSelection();
            var selectedText = string.Join(Environment.NewLine, selectedRanges.Select(range => range.GetText(-1)));
            return string.IsNullOrWhiteSpace(selectedText) ? null : selectedText;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ElementNotAvailableException or COMException)
        {
            return null;
        }
    }
}
