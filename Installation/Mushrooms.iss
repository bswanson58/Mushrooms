; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define ApplicationName "Mushrooms"
#define ApplicationVersion "v" + GetFileVersion("..\Mushrooms\bin\Release\net7.0-windows\Mushrooms.exe")
#define GroupName "Secret Squirrel Software"
#define ExecutableName "Mushrooms.exe"
#define Platform "Any"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{BC32CFA3-1CE4-43AB-8124-3231BE5E93EA}}
AppName={#ApplicationName}
AppVersion={#ApplicationVersion}
AppVerName={#ApplicationName} {#ApplicationVersion}
AppPublisher={#GroupName}
DefaultDirName={commonpf}\{#GroupName}\{#ApplicationName}
DefaultGroupName={#ApplicationName}
AllowNoIcons=yes
LicenseFile=license.rtf
OutputBaseFilename=Setup_{#ApplicationName}_{#ApplicationVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=X64
SetupIconFile=..\Mushrooms\Resources\Mushroom.ico
UninstallDisplayIcon={app}\{#ExecutableName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "..\Mushrooms\bin\Release\net7.0-windows\ColorMinePortable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\ColorThief.ImageSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\ColorThief.ImageSharp.Shared.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\ControlzEx.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\DynamicData.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Fluxor.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Gu.Wpf.Geometry.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\HueLighting.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\LiteDB.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.Binder.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.CommandLine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.EnvironmentVariables.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.FileExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Configuration.UserSecrets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.DependencyInjection.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.DependencyInjection.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.DependencyModel.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.FileProviders.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.FileProviders.Physical.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.FileSystemGlobbing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Hosting.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Hosting.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Logging.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Logging.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Logging.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Logging.Debug.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Logging.EventLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Logging.EventSource.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Options.ConfigurationExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Options.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Extensions.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Mushrooms.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Mushrooms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Mushrooms.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Mushrooms.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Mushrooms.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Mushrooms.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\NCuid.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Q42.HueApi.ColorConverters.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Q42.HueApi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\ReusableBits.Platform.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\ReusableBits.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Scrutor.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Serilog.Enrichers.Process.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Serilog.Sinks.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\Serilog.Sinks.RollingFile.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\SixLabors.ImageSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Mushrooms\bin\Release\net7.0-windows\System.Reactive.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"
Name: "{commondesktop}\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#ExecutableName}"; Description: "{cm:LaunchProgram,{#StringChange(ApplicationName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsAppRunning(const FileName : string): Boolean;
var
    FSWbemLocator: Variant;
    FWMIService   : Variant;
    FWbemObjectSet: Variant;
begin
    Result := false;
    FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
    FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
    FWbemObjectSet :=
      FWMIService.ExecQuery(
        Format('SELECT Name FROM Win32_Process Where Name="%s"', [FileName]));
    Result := (FWbemObjectSet.Count > 0);
    FWbemObjectSet := Unassigned;
    FWMIService := Unassigned;
    FSWbemLocator := Unassigned;
end;

function InitializeSetup: boolean;
begin
  Result := not IsAppRunning('Mushrooms.exe');
  if not Result then
  MsgBox('Mushrooms is running. Please close the application before running the installer ', mbError, MB_OK);
end;