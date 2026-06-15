namespace ClipSpeak;

internal sealed class SelectedTextReader
{
    private const int PrimaryCopyTimeoutMilliseconds = 450;
    private const int FallbackCopyTimeoutMilliseconds = 900;
    private const int ClipboardPollIntervalMilliseconds = 50;

    public SelectedTextResult TryGetSelectedText(bool clearClipboardAfterReading, IntPtr targetWindow = default)
    {
        System.Windows.Forms.IDataObject? previousClipboard = null;
        var clipboardCleanedUp = true;

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
                if (clearClipboardAfterReading)
                {
                    ClearClipboardSoon();
                }
                else
                {
                    RestoreClipboardSoon(previousClipboard);
                }

                return new SelectedTextResult(text, ClipboardCleanedUp: true);
            }

            clipboardCleanedUp = TryRestoreClipboard(previousClipboard);
            return new SelectedTextResult(null, clipboardCleanedUp);
        }
        catch
        {
            clipboardCleanedUp = TryRestoreClipboard(previousClipboard);
            return new SelectedTextResult(null, clipboardCleanedUp);
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

    private static void ClearClipboardSoon()
    {
        var timer = new System.Windows.Forms.Timer { Interval = 250 };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            _ = TryClearClipboard();
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

    private static bool TryClearClipboard()
    {
        try
        {
            Clipboard.Clear();
            return true;
        }
        catch (Exception ex) when (ex is ExternalException or ThreadStateException)
        {
            return false;
        }
    }
}

internal sealed record SelectedTextResult(string? Text, bool ClipboardCleanedUp);
