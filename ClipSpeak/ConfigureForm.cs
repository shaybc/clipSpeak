namespace ClipSpeak;

internal sealed class ConfigureForm : Form
{
    private readonly HotkeyBox _readHotkeyBox;
    private readonly HotkeyBox _stopHotkeyBox;

    public AppSettings Settings { get; private set; }

    public ConfigureForm(AppSettings currentSettings)
    {
        Settings = new AppSettings
        {
            ReadHotkey = currentSettings.ReadHotkey,
            StopHotkey = currentSettings.StopHotkey
        };

        Text = "Configure ClipSpeak";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(420, 188);

        var readLabel = new Label
        {
            Text = "Read clipboard",
            AutoSize = true,
            Location = new Point(20, 24)
        };

        _readHotkeyBox = new HotkeyBox
        {
            Hotkey = Settings.ReadHotkey,
            Location = new Point(150, 20),
            Width = 240
        };

        var stopLabel = new Label
        {
            Text = "Pause or stop",
            AutoSize = true,
            Location = new Point(20, 68)
        };

        _stopHotkeyBox = new HotkeyBox
        {
            Hotkey = Settings.StopHotkey,
            Location = new Point(150, 64),
            Width = 240
        };

        var hint = new Label
        {
            Text = "Click a field, then press the hotkey combination.",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(20, 108)
        };

        var saveButton = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            Location = new Point(234, 144),
            Size = new Size(75, 28)
        };
        saveButton.Click += (_, _) => SaveSettings();

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(315, 144),
            Size = new Size(75, 28)
        };

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        Controls.AddRange([
            readLabel,
            _readHotkeyBox,
            stopLabel,
            _stopHotkeyBox,
            hint,
            saveButton,
            cancelButton
        ]);
    }

    private void SaveSettings()
    {
        if (!_readHotkeyBox.Hotkey.IsValid || !_stopHotkeyBox.Hotkey.IsValid)
        {
            MessageBox.Show(this, "Both hotkeys need a modifier and a key.", "ClipSpeak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        if (_readHotkeyBox.Hotkey == _stopHotkeyBox.Hotkey)
        {
            MessageBox.Show(this, "Choose two different hotkeys.", "ClipSpeak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        Settings = new AppSettings
        {
            ReadHotkey = _readHotkeyBox.Hotkey,
            StopHotkey = _stopHotkeyBox.Hotkey
        };
    }
}
