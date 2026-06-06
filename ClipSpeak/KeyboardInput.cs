namespace ClipSpeak;

internal static class KeyboardInput
{
    private const ushort VkControl = 0x11;
    private const ushort VkShift = 0x10;
    private const ushort VkMenu = 0x12;
    private const ushort VkLWin = 0x5B;
    private const ushort VkRWin = 0x5C;
    private const ushort VkC = 0x43;
    private const uint InputKeyboard = 1;
    private const uint KeyEventFKeyUp = 0x0002;

    public static void SendCopyShortcut()
    {
        // Release common hotkey modifiers before sending Ctrl+C to the foreground app.
        SendKeyUp(VkControl);
        SendKeyUp(VkShift);
        SendKeyUp(VkMenu);
        SendKeyUp(VkLWin);
        SendKeyUp(VkRWin);
        Thread.Sleep(80);

        SendKeyboardInputs(
            KeyDown(VkControl),
            KeyDown(VkC),
            KeyUp(VkC),
            KeyUp(VkControl));
    }

    private static void SendKeyUp(ushort virtualKey)
    {
        SendKeyboardInputs(KeyUp(virtualKey));
    }

    private static void SendKeyboardInputs(params Input[] inputs)
    {
        _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
    }

    private static Input KeyDown(ushort virtualKey) => new()
    {
        Type = InputKeyboard,
        Data = new InputUnion
        {
            Keyboard = new KeyboardInputData
            {
                VirtualKey = virtualKey
            }
        }
    };

    private static Input KeyUp(ushort virtualKey)
    {
        var input = KeyDown(virtualKey);
        input.Data.Keyboard.Flags = KeyEventFKeyUp;
        return input;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KeyboardInputData Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInputData
    {
        public ushort VirtualKey;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
}
