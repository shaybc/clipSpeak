namespace ClipSpeak;

internal sealed class ConfigureForm : Form
{
    private readonly HotkeyBox _readHotkeyBox;
    private readonly HotkeyBox _readSelectionHotkeyBox;
    private readonly HotkeyBox _stopHotkeyBox;
    private readonly CheckBox _showSelectedTextMouseMenuCheckBox;

    public AppSettings Settings { get; private set; }

    public ConfigureForm(AppSettings currentSettings)
    {
        Settings = new AppSettings
        {
            ReadHotkey = currentSettings.ReadHotkey,
            ReadSelectionHotkey = currentSettings.ReadSelectionHotkey,
            StopHotkey = currentSettings.StopHotkey,
            ShowSelectedTextMouseMenu = currentSettings.ShowSelectedTextMouseMenu
        };

        Text = "Configure ClipSpeak";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(440, 306);

        var readLabel = new Label
        {
            Text = "Read clipboard",
            AutoSize = true,
            Location = new Point(20, 24)
        };

        _readHotkeyBox = new HotkeyBox
        {
            Hotkey = Settings.ReadHotkey,
            Location = new Point(170, 20),
            Width = 240
        };

        var readSelectionLabel = new Label
        {
            Text = "Read selected text",
            AutoSize = true,
            Location = new Point(20, 68)
        };

        _readSelectionHotkeyBox = new HotkeyBox
        {
            Hotkey = Settings.ReadSelectionHotkey,
            Location = new Point(170, 64),
            Width = 240
        };

        var stopLabel = new Label
        {
            Text = "Pause or stop",
            AutoSize = true,
            Location = new Point(20, 112)
        };

        _stopHotkeyBox = new HotkeyBox
        {
            Hotkey = Settings.StopHotkey,
            Location = new Point(170, 108),
            Width = 240
        };

        var hint = new Label
        {
            Text = "Click a field, then press the hotkey combination.",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(20, 152)
        };

        _showSelectedTextMouseMenuCheckBox = new CheckBox
        {
            Text = "Show ClipSpeak menu on Ctrl + Right Click",
            AutoSize = true,
            Checked = Settings.ShowSelectedTextMouseMenu,
            Location = new Point(20, 184)
        };

        var saveButton = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            Location = new Point(254, 262),
            Size = new Size(75, 28)
        };
        saveButton.Click += (_, _) => SaveSettings();

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(335, 262),
            Size = new Size(75, 28)
        };

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        Controls.AddRange([
            readLabel,
            _readHotkeyBox,
            readSelectionLabel,
            _readSelectionHotkeyBox,
            stopLabel,
            _stopHotkeyBox,
            hint,
            _showSelectedTextMouseMenuCheckBox,
            saveButton,
            cancelButton
        ]);
    }

    private void SaveSettings()
    {
        var hotkeys = new[]
        {
            _readHotkeyBox.Hotkey,
            _readSelectionHotkeyBox.Hotkey,
            _stopHotkeyBox.Hotkey
        };

        if (hotkeys.Any(hotkey => !hotkey.IsValid))
        {
            MessageBox.Show(this, "All hotkeys need a modifier and a key.", "ClipSpeak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        if (hotkeys.Distinct().Count() != hotkeys.Length)
        {
            MessageBox.Show(this, "Choose different hotkeys for each action.", "ClipSpeak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        Settings = new AppSettings
        {
            ReadHotkey = _readHotkeyBox.Hotkey,
            ReadSelectionHotkey = _readSelectionHotkeyBox.Hotkey,
            StopHotkey = _stopHotkeyBox.Hotkey,
            ShowSelectedTextMouseMenu = _showSelectedTextMouseMenuCheckBox.Checked
        };
    }
}
