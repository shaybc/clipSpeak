namespace ClipSpeak;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly GlobalHotkeyManager _hotkeys = new();
    private readonly SpeechService _speech = new();
    private readonly ClipboardReader _clipboard = new();
    private readonly SelectedTextReader _selectedText = new();
    private readonly GlobalMouseMenu _mouseMenu;
    private readonly ContextMenuStrip _selectedTextContextMenu;
    private readonly Icon _appIcon;
    private AppSettings _settings;
    private IntPtr _selectedTextMouseTargetWindow;
    private bool _readingSelectedText;

    public TrayApplicationContext()
    {
        _settings = AppSettings.Load();
        _appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        _selectedTextContextMenu = BuildSelectedTextContextMenu();
        _mouseMenu = new GlobalMouseMenu(ShowSelectedTextMouseMenu);
        _notifyIcon = BuildNotifyIcon();
        _notifyIcon.Visible = true;
        _mouseMenu.Enabled = _settings.ShowSelectedTextMouseMenu;

        if (_settings.ShowSelectedTextMouseMenu && !_mouseMenu.IsInstalled)
        {
            ShowBalloon("Mouse menu unavailable", "ClipSpeak could not install the Ctrl + Right Click mouse menu hook.");
        }

        if (!_speech.IsAvailable)
        {
            ShowBalloon("Speech is unavailable", "Windows SAPI speech synthesis could not be started.");
        }

        RegisterHotkeys();
    }

    private NotifyIcon BuildNotifyIcon()
    {
        var menu = MenuStyling.CreateMenu();
        menu.Items.Add(MenuStyling.CreateItem("Configure", MenuIconKind.Settings, (_, _) => ShowConfigureDialog()));
        menu.Items.Add(MenuStyling.CreateItem("Read selected text", MenuIconKind.Speak, (_, _) => ReadSelectedTextAloud()));
        menu.Items.Add(MenuStyling.CreateItem("Help", MenuIconKind.Help, (_, _) => ShowHelpDialog()));
        menu.Items.Add(MenuStyling.CreateItem("About", MenuIconKind.Info, (_, _) => ShowAboutDialog()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(MenuStyling.CreateItem("Exit", MenuIconKind.Exit, (_, _) => ExitThread()));

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
        _mouseMenu.Enabled = _settings.ShowSelectedTextMouseMenu;
        RegisterHotkeys();
    }

    private void ShowHelpDialog()
    {
        using var dialog = new HelpForm(_appIcon, _settings);
        dialog.ShowDialog();
    }

    private void ShowAboutDialog()
    {
        using var dialog = new AboutForm(_appIcon);
        dialog.ShowDialog();
    }

    private ContextMenuStrip BuildSelectedTextContextMenu()
    {
        var menu = MenuStyling.CreateMenu();
        menu.Items.Add(MenuStyling.CreateItem("ClipSpeak selected text", MenuIconKind.Speak, (_, _) => ReadSelectedTextAloud(_selectedTextMouseTargetWindow)));
        return menu;
    }

    private void ShowSelectedTextMouseMenu(Point location, IntPtr targetWindow)
    {
        _selectedTextMouseTargetWindow = targetWindow;
        _selectedTextContextMenu.Show(location);
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

        _speech.SpeakAsync(PrepareTextForSpeech(text));
    }

    private void ReadSelectedTextAloud()
    {
        ReadSelectedTextAloud(ForegroundWindow.Current);
    }

    private void ReadSelectedTextAloud(IntPtr targetWindow)
    {
        if (_readingSelectedText)
        {
            return;
        }

        _readingSelectedText = true;
        try
        {
            string? text;
            try
            {
                text = _selectedText.TryGetSelectedText(targetWindow);
            }
            catch
            {
                ShowBalloon("Could not read selection", "ClipSpeak hit an unexpected error while reading selected text.");
                return;
            }

            if (text is null)
            {
                ShowBalloon("No selected text found", "Select text in the focused app, then use the selected-text hotkey.");
                return;
            }

            _speech.SpeakAsync(PrepareTextForSpeech(text));
        }
        finally
        {
            _readingSelectedText = false;
        }
    }

    private string PrepareTextForSpeech(string text)
    {
        return _settings.SuppressUrlReading ? SpeechTextSanitizer.SuppressUrls(text) : text;
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
        _mouseMenu.Dispose();
        _selectedTextContextMenu.Dispose();
        _notifyIcon.Dispose();
        _appIcon.Dispose();
        _speech.Dispose();
        _hotkeys.Dispose();
        base.ExitThreadCore();
    }
}
