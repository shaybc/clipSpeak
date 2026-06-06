namespace ClipSpeak;

internal static class Program
{
    private const string SingleInstanceMutexName = @"Global\ClipSpeak_3B7BD317_F092_49F3_9B0E_61E42D873948";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        using var app = new TrayApplicationContext();
        Application.Run(app);
    }
}
