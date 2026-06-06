using System.Text.Json;

namespace ClipSpeak;

internal sealed class AppSettings
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public HotkeyDefinition ReadHotkey { get; set; } = new(Keys.C, HotkeyModifiers.Control | HotkeyModifiers.Alt);
    public HotkeyDefinition StopHotkey { get; set; } = new(Keys.S, HotkeyModifiers.Control | HotkeyModifiers.Alt);

    public static string SettingsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClipSpeak");

    public static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, SerializerOptions));
    }
}
