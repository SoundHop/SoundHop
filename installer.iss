; SoundHop Installer Script for Inno Setup
; https://jrsoftware.org/isinfo.php

#define MyAppName "SoundHop"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "SoundHop"
#define MyAppURL "https://github.com/SoundHop/SoundHop"
#define MyAppExeName "SoundHop.exe"

[Setup]
; NOTE: AppId uniquely identifies this application. Do not use the same AppId in other applications.
AppId={{8A7B9C6D-5E4F-3A2B-1C0D-9E8F7A6B5C4D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; Require admin rights for Program Files installation
PrivilegesRequired=admin
; Output settings
OutputDir=installer-output
OutputBaseFilename=SoundHop_Setup_{#MyAppVersion}
; Icon settings
SetupIconFile=assets\app_icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
; Compression
Compression=lzma2
SolidCompression=yes
; Visual settings
WizardStyle=modern
; Minimum Windows version (Windows 10 1903)
MinVersion=10.0.18362

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "Start SoundHop when Windows starts"; GroupDescription: "Windows Startup:"; Flags: unchecked

[Files]
; Include all files from the publish output directory
Source: "AudioSwitcher.UI\bin\x64\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Start Menu shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
; Desktop shortcut (optional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Startup registry entry (optional)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

[Run]
; Option to launch app after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
