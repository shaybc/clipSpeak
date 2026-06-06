namespace ClipSpeak;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly GlobalHotkeyManager _hotkeys = new();
    private readonly SpeechService _speech = new();
    private readonly ClipboardReader _clipboard = new();
    private readonly SelectedTextReader _selectedText = new();
    private readonly Icon _appIcon;
    private AppSettings _settings;

    public TrayApplicationContext()
    {
        _settings = AppSettings.Load();
        _appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        _notifyIcon = BuildNotifyIcon();
        _notifyIcon.Visible = true;

        if (!_speech.IsAvailable)
        {
            ShowBalloon("Speech is unavailable", "Windows SAPI speech synthesis could not be started.");
        }

        RegisterHotkeys();
    }

    private NotifyIcon BuildNotifyIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Configure", null, (_, _) => ShowConfigureDialog());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());

        var icon = new NotifyIcon
        {
            Text = "ClipSpeak",
            Icon = _appIcon,
            ContextMenuStrip = menu
        };

        icon.DoubleClick += (_, _) => ShowConfigureDialog();
        return icon;
    }

    private void ShowConfigureDialog()
    {
        using var dialog = new ConfigureForm(_settings);
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _settings = dialog.Settings;
        _settings.Save();
        RegisterHotkeys();
    }

    private void RegisterHotkeys()
    {
        _hotkeys.Clear();
        var errors = new List<string>();

        if (!_hotkeys.Register(_settings.ReadHotkey, ReadClipboardAloud, out var readError) && readError is not null)
        {
            errors.Add($"Read clipboard: {readError}");
        }

        if (!_hotkeys.Register(_settings.ReadSelectionHotkey, ReadSelectedTextAloud, out var readSelectionError) && readSelectionError is not null)
        {
            errors.Add($"Read selected text: {readSelectionError}");
        }

        if (!_hotkeys.Register(_settings.StopHotkey, StopReading, out var stopError) && stopError is not null)
        {
            errors.Add($"Pause/stop reading: {stopError}");
        }

        if (errors.Count > 0)
        {
            ShowBalloon("Hotkey registration issue", string.Join(Environment.NewLine, errors));
        }
    }

    private void ReadClipboardAloud()
    {
        var text = _clipboard.TryGetText();
        if (text is null)
        {
            ShowBalloon("Nothing to read", "The clipboard does not contain readable text.");
            return;
        }

        _speech.SpeakAsync(text);
    }

    private void ReadSelectedTextAloud()
    {
        var result = _selectedText.TryGetSelectedText();
        if (result.Text is null)
        {
            ShowBalloon("No selected text found", "Select text in the focused app, then use the selected-text hotkey.");
            return;
        }

        if (!result.ClipboardRestored)
        {
            ShowBalloon("Clipboard restore issue", "ClipSpeak read the selected text, but could not restore the previous clipboard contents.");
        }

        _speech.SpeakAsync(result.Text);
    }

    private void StopReading()
    {
        _speech.Stop();
    }

    private void ShowBalloon(string title, string message)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(4000);
    }

    protected override void ExitThreadCore()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _appIcon.Dispose();
        _speech.Dispose();
        _hotkeys.Dispose();
        base.ExitThreadCore();
    }
}
