@echo off
setlocal

set "ROOT=%~dp0"
set "PROJECT=%ROOT%ClipSpeak\ClipSpeak.csproj"
set "SOLUTION=%ROOT%ClipSpeak.sln"
set "PUBLISH_DIR=%ROOT%artifacts\publish\ClipSpeak"
set "INSTALLER_SCRIPT=%ROOT%installer\ClipSpeak.iss"
set "ISCC="

where ISCC.exe >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    for /f "delims=" %%I in ('where ISCC.exe') do (
        set "ISCC=%%I"
        goto :found_iscc
    )
)

if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" (
    set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
    goto :found_iscc
)

if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" (
    set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"
    goto :found_iscc
)

if exist "%LocalAppData%\Programs\Inno Setup 6\ISCC.exe" (
    set "ISCC=%LocalAppData%\Programs\Inno Setup 6\ISCC.exe"
    goto :found_iscc
)

echo Inno Setup 6 compiler was not found.
echo Install Inno Setup 6 or add ISCC.exe to PATH.
exit /b 1

:found_iscc
echo Using Inno Setup compiler: %ISCC%
echo.

echo Building ClipSpeak...
dotnet build "%SOLUTION%" -c Release
if %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

echo.
echo Publishing ClipSpeak...
dotnet publish "%PROJECT%" -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%"
if %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

echo.
echo Building installer...
"%ISCC%" "%INSTALLER_SCRIPT%"
if %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

echo.
echo Build complete.
echo Installer: %ROOT%artifacts\installer\ClipSpeakSetup.exe
exit /b 0
