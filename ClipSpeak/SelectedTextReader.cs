namespace ClipSpeak;

internal sealed class SelectedTextReader
{
    private const int PrimaryCopyTimeoutMilliseconds = 450;
    private const int FallbackCopyTimeoutMilliseconds = 900;
    private const int ClipboardPollIntervalMilliseconds = 50;

    public SelectedTextResult TryGetSelectedText(IntPtr targetWindow = default)
    {
        System.Windows.Forms.IDataObject? previousClipboard = null;
        var clipboardRestored = true;

        try
        {
            ForegroundWindow.TryActivate(targetWindow);
            previousClipboard = Clipboard.GetDataObject();
            Clipboard.Clear();

            KeyboardInput.SendCopyShortcutWithSendKeys();
            var text = WaitForClipboardText(PrimaryCopyTimeoutMilliseconds);
            if (string.IsNullOrWhiteSpace(text))
            {
                KeyboardInput.SendCopyShortcut();
                text = WaitForClipboardText(FallbackCopyTimeoutMilliseconds);
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                RestoreClipboardSoon(previousClipboard);
                return new SelectedTextResult(text, ClipboardRestored: true);
            }

            clipboardRestored = TryRestoreClipboard(previousClipboard);
            return new SelectedTextResult(null, clipboardRestored);
        }
        catch
        {
            clipboardRestored = TryRestoreClipboard(previousClipboard);
            return new SelectedTextResult(null, clipboardRestored);
        }
    }

    private static string? WaitForClipboardText(int timeoutMilliseconds)
    {
        var deadline = Environment.TickCount64 + timeoutMilliseconds;
        while (Environment.TickCount64 < deadline)
        {
            try
            {
                if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
                {
                    return Clipboard.GetText(TextDataFormat.UnicodeText);
                }
            }
            catch (Exception ex) when (ex is ExternalException or ThreadStateException)
            {
                return null;
            }

            Thread.Sleep(ClipboardPollIntervalMilliseconds);
            Application.DoEvents();
        }

        return null;
    }

    private static void RestoreClipboardSoon(System.Windows.Forms.IDataObject? previousClipboard)
    {
        var timer = new System.Windows.Forms.Timer { Interval = 250 };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            _ = TryRestoreClipboard(previousClipboard);
            timer.Dispose();
        };
        timer.Start();
    }

    private static bool TryRestoreClipboard(System.Windows.Forms.IDataObject? previousClipboard)
    {
        try
        {
            if (previousClipboard is null)
            {
                Clipboard.Clear();
            }
            else
            {
                Clipboard.SetDataObject(previousClipboard, copy: true);
            }

            return true;
        }
        catch (Exception ex) when (ex is ExternalException or ThreadStateException)
        {
            return false;
        }
    }
}

internal sealed record SelectedTextResult(string? Text, bool ClipboardRestored);
