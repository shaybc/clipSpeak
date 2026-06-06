using System.Diagnostics;
using System.Reflection;

namespace ClipSpeak;

internal sealed class AboutForm : Form
{
    private const string RepositoryUrl = "https://github.com/shaybc/clipSpeak";

    public AboutForm(Icon appIcon)
    {
        Text = "About ClipSpeak";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(420, 190);
        Icon = appIcon;

        var iconBox = new PictureBox
        {
            Image = appIcon.ToBitmap(),
            Location = new Point(20, 22),
            Size = new Size(48, 48),
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        var titleLabel = new Label
        {
            Text = "ClipSpeak",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(86, 20)
        };

        var versionLabel = new Label
        {
            Text = $"Version {GetVersion()}",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(86, 46)
        };

        var descriptionLabel = new Label
        {
            Text = "Reads clipboard or selected text aloud using the default Windows speech voice.",
            AutoSize = false,
            Location = new Point(86, 76),
            Size = new Size(300, 38)
        };

        var linkLabel = new LinkLabel
        {
            Text = RepositoryUrl,
            AutoSize = true,
            Location = new Point(86, 122)
        };
        linkLabel.LinkClicked += (_, _) => OpenRepository();

        var closeButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(325, 146),
            Size = new Size(75, 28)
        };

        AcceptButton = closeButton;
        CancelButton = closeButton;

        Controls.AddRange([
            iconBox,
            titleLabel,
            versionLabel,
            descriptionLabel,
            linkLabel,
            closeButton
        ]);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (Control control in Controls)
            {
                if (control is PictureBox { Image: not null } pictureBox)
                {
                    pictureBox.Image.Dispose();
                }
            }
        }

        base.Dispose(disposing);
    }

    private static string GetVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "unknown" : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private static void OpenRepository()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RepositoryUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            Clipboard.SetText(RepositoryUrl);
            MessageBox.Show("The GitHub link was copied to the clipboard.", "ClipSpeak", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
