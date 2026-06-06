using System.Reflection;

namespace ClipSpeak;

internal sealed class HelpForm : Form
{
    public HelpForm(Icon appIcon, AppSettings settings)
    {
        Text = "ClipSpeak Help";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(560, 460);
        Icon = appIcon;

        var content = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            ScrollBars = ScrollBars.Vertical,
            Location = new Point(16, 16),
            Size = new Size(528, 386),
            Text = BuildHelpText(settings)
        };

        var closeButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(469, 416),
            Size = new Size(75, 28)
        };

        AcceptButton = closeButton;
        CancelButton = closeButton;

        Controls.AddRange([
            content,
            closeButton
        ]);
    }

    private static string BuildHelpText(AppSettings settings)
    {
        return string.Join(Environment.NewLine, [
            "ClipSpeak Help",
            $"Version {GetVersion()}",
            "",
            "What ClipSpeak does",
            "ClipSpeak runs in the Windows notification area and reads text aloud using your default Windows speech voice.",
            "",
            "Default actions",
            $"Read clipboard: {settings.ReadHotkey}",
            $"Read selected text: {settings.ReadSelectionHotkey}",
            $"Pause or stop reading: {settings.StopHotkey}",
            "",
            "Reading clipboard text",
            "Copy any text to the clipboard, then press the Read clipboard hotkey. ClipSpeak reads the clipboard aloud.",
            "",
            "Reading selected text",
            "Select text in the focused app, then press the Read selected text hotkey. ClipSpeak temporarily copies the selection, reads it aloud, and restores your previous clipboard contents when possible.",
            "",
            "Mouse popup",
            "Hold Ctrl and right-click selected text to show the ClipSpeak popup menu, then choose ClipSpeak selected text. This is a ClipSpeak menu, not an item added to each app's own right-click menu. A normal right-click still shows the app's own menu.",
            "",
            "If selected text cannot be copied",
            "Some apps block simulated copy input or run with higher privileges than ClipSpeak. If that happens, ClipSpeak shows a tray notice and does not read old clipboard text by mistake.",
            "",
            "Configuring hotkeys",
            "Right-click the ClipSpeak tray icon and choose Configure. Click a hotkey field, press the new key combination, then choose Save. Each hotkey must use a modifier such as Ctrl, Alt, or Shift, and all hotkeys must be different.",
            "",
            "Setting up Windows speech",
            "ClipSpeak uses the built-in Windows speech engine. You do not need to keep Narrator running, but Narrator settings are a good place to test that speech works.",
            "",
            "To choose or download a voice:",
            "1. Open Windows Settings.",
            "2. Go to Accessibility > Narrator and choose a voice under Narrator voice.",
            "3. To add more voices, go to Time & language > Speech and download voices for the language you want.",
            "4. Test the voice in Windows settings, then use ClipSpeak again.",
            "",
            "Exiting ClipSpeak",
            "Right-click the tray icon and choose Exit."
        ]);
    }

    private static string GetVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "unknown" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
