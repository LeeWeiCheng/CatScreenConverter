#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish\win-x64"
#endif
#ifndef ReleaseDir
  #define ReleaseDir "..\artifacts\release"
#endif

[Setup]
AppId={{5D99641E-06D9-4AE9-B4F3-BFEA8A86210C}
AppName=Cat螢幕轉換器
AppVersion={#AppVersion}
AppPublisher=QxQ
AppComments=提供貓專用的近似色差顯示
DefaultDirName={localappdata}\Programs\QxQ\CatScreenConverter
DefaultGroupName=QxQ\Cat螢幕轉換器
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
MinVersion=10.0.19045
OutputDir={#ReleaseDir}
OutputBaseFilename=QxQ-CatScreenConverter-Setup-{#AppVersion}-win-x64
SetupIconFile=..\Assets\CatScreen.ico
UninstallDisplayIcon={app}\CatVideo.exe
VersionInfoCompany=QxQ
VersionInfoDescription=Cat螢幕轉換器安裝程式
VersionInfoVersion={#AppVersion}
VersionInfoProductName=Cat螢幕轉換器
VersionInfoProductVersion={#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
AppMutex=Local\CatVideo.DisplayColor
CloseApplications=no
RestartApplications=no
SetupLogging=yes

[Languages]
Name: "chinesetraditional"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "建立桌面捷徑"; GroupDescription: "讓貓咪更快上工："; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{group}\Cat螢幕轉換器"; Filename: "{app}\CatVideo.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\Cat螢幕轉換器"; Filename: "{app}\CatVideo.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\CatVideo.exe"; Description: "啟動 Cat螢幕轉換器，準備值班"; Flags: nowait postinstall skipifsilent
