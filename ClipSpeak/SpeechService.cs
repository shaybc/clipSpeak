using System.Reflection;

namespace ClipSpeak;

internal sealed class SpeechService : IDisposable
{
    private readonly object? _voice;
    private readonly Type? _voiceType;
    private bool _disposed;

    public SpeechService()
    {
        _voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
        _voice = _voiceType is null ? null : Activator.CreateInstance(_voiceType);
    }

    public bool IsAvailable => _voice is not null && _voiceType is not null;

    public void SpeakAsync(string text)
    {
        if (!IsAvailable)
        {
            return;
        }

        Stop();
        Invoke("Speak", text, (int)(SpeechVoiceSpeakFlags.Async | SpeechVoiceSpeakFlags.PurgeBeforeSpeak));
    }

    public void Stop()
    {
        if (!IsAvailable)
        {
            return;
        }

        Invoke("Speak", string.Empty, (int)(SpeechVoiceSpeakFlags.Async | SpeechVoiceSpeakFlags.PurgeBeforeSpeak));
    }

    private object? Invoke(string methodName, params object[] args)
    {
        return _voiceType!.InvokeMember(
            methodName,
            BindingFlags.InvokeMethod,
            binder: null,
            target: _voice,
            args: args);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        if (_voice is not null)
        {
            Marshal.FinalReleaseComObject(_voice);
        }

        _disposed = true;
    }
}

[Flags]
internal enum SpeechVoiceSpeakFlags
{
    Async = 1,
    PurgeBeforeSpeak = 2
}
