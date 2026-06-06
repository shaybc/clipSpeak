# ClipSpeak

ClipSpeak is a Windows tray application that reads the current clipboard text aloud using the user's default Windows speech voice.

## Features

- Runs in the notification area.
- Global hotkey to read the clipboard.
- Global hotkey to pause or stop speech.
- Configure dialog available from the tray icon.
- Settings are saved under `%APPDATA%\ClipSpeak`.
- Inno Setup installer script for Program Files installation, Start Menu shortcuts, and uninstall support.

## Default Hotkeys

- Read clipboard: `Ctrl + Alt + C`
- Pause or stop reading: `Ctrl + Alt + S`

## Build

```powershell
dotnet build .\ClipSpeak.sln -c Release
```

## Publish

```powershell
dotnet publish .\ClipSpeak\ClipSpeak.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\publish\ClipSpeak
```

## Installer

Install Inno Setup, then compile:

```powershell
iscc .\installer\ClipSpeak.iss
```

The installer is written to `artifacts\installer\ClipSpeakSetup.exe`.
