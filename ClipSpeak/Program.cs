namespace ClipSpeak;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var app = new TrayApplicationContext();
        Application.Run(app);
    }
}
