# ClipSpeak

ClipSpeak is a Windows tray application that reads the current clipboard text aloud using the user's default Windows speech voice.

Current version: `1.5.0`

## Features

- Runs in the notification area.
- Global hotkey to read the clipboard.
- Global hotkey to read selected text from the focused app.
- `Ctrl + Right Click` popup menu to read selected text.
- Optional cleanup to remove selected text from the clipboard after it is sent to speech.
- Global hotkey to pause or stop speech.
- Configure, Help, and About dialogs available from the tray icon.
- Dark tray menus with small action icons.
- Settings are saved under `%APPDATA%\ClipSpeak`.
- Inno Setup installer script for Program Files installation, Start Menu shortcuts, and uninstall support.

## Minimum Requirements

- Windows 10 or Windows 11, 64-bit.
- At least one installed Windows text-to-speech voice.
- A default Windows speech voice selected.
- Clipboard text to read.
- Selected text in an app that supports copying with `Ctrl+C`, when using selected-text reading.

ClipSpeak uses the built-in Windows speech engine. You do not need to keep Narrator running, but Narrator or Windows speech settings are the usual place to confirm that speech output works.

To select or download a voice:

1. Open Windows Settings.
2. Go to `Accessibility` > `Narrator` and choose a voice under Narrator voice.
3. If you need more voices, go to `Time & language` > `Speech`, then add or download voices for the language you want.
4. Test the selected voice in Windows settings before using ClipSpeak.

## Default Hotkeys

- Read clipboard: `Ctrl + Alt + C`
- Read selected text: `Ctrl + Alt + R`
- Pause or stop reading: `Ctrl + Alt + S`

You can also hold `Ctrl` and right-click selected text to open the ClipSpeak popup menu, then choose `ClipSpeak selected text`.

## Build

### Build Requirements

- Windows 10 or Windows 11.
- .NET 8 SDK with the Windows Desktop workload/runtime.
- Inno Setup 6 to compile the installer.
- PowerShell or another Windows shell for the build commands below.

The app targets `net8.0-windows` and uses Windows Forms, clipboard APIs, global hotkeys, and the built-in Windows speech engine, so it must be built on Windows.

Use the root build script to build, publish, and compile the installer:

```cmd
build.bat
```

Or build manually:

```powershell
dotnet build .\ClipSpeak.sln -c Release
dotnet publish .\ClipSpeak\ClipSpeak.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\publish\ClipSpeak
iscc .\installer\ClipSpeak.iss
```

## Installer

The installer is written to `artifacts\installer\ClipSpeakSetup.exe`.
