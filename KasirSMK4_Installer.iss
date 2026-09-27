; =====================================================================
; SKRIP INNO SETUP INSTALLER RESMI
; APLIKASI KASIR POS SMK NEGERI 4 KABUPATEN TANGERANG
; =====================================================================

#define MyAppName "Aplikasi Kasir SMK Negeri 4"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "SMK Negeri 4 Kabupaten Tangerang"
#define MyAppExeName "AplikasiKasirSMK4.exe"
#define MySourceDir "publish\KasirSMK4"

[Setup]
; Informasi Aplikasi
AppId={{E8F91B2C-94F3-4A1D-A5F1-8B39B2A7C310}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Kasir SMK Negeri 4
DefaultGroupName=Kasir SMK Negeri 4
DisableProgramGroupPage=no
OutputBaseFilename=Setup_KasirSMK4_v1.0.0
OutputDir=publish\Installer
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=Resources\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
; File utama aplikasi dan dependensi
Source: "{#MySourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MySourceDir}\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MySourceDir}\skema.sqlite.sql"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MySourceDir}\PETUNJUK_PENGGUNAAN.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MySourceDir}\Jalankan_Kasir.bat"; DestDir: "{app}"; Flags: ignoreversion

; Folder Migrasi
Source: "{#MySourceDir}\migrasi\*"; DestDir: "{app}\migrasi"; Flags: ignoreversion recursesubdirs createallsubdirs

; Folder Resources (Icon dan Logo)
Source: "{#MySourceDir}\Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion recursesubdirs createallsubdirs

; Folder Database - onlyinstallifdoesntexist agar database yang sudah ada tidak tertimpa saat update
Source: "{#MySourceDir}\data\kasir.db"; DestDir: "{app}\data"; Flags: onlyifdoesntexist uninsneveruninstall

[Icons]
; Shortcut di Start Menu
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Resources\app.ico"
Name: "{group}\Panduan Penggunaan"; Filename: "{app}\PETUNJUK_PENGGUNAAN.txt"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Shortcut di Desktop
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Resources\app.ico"; Tasks: desktopicon

[Run]
; Opsi jalankan setelah instalasi selesai
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
