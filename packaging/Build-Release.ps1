param(
    [string]$Version,
    [string]$InnoCompiler,
    [string]$RestoreSource,
    [string]$PackagesPath,
    [string]$ReleaseDirectory
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$projectFile = Join-Path $projectRoot 'CatVideo.csproj'
if (-not $Version) { $Version = ([xml](Get-Content -LiteralPath $projectFile -Raw)).Project.PropertyGroup.Version }
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must have the form 1.0.0.' }
$publishDir = Join-Path $projectRoot 'artifacts\publish\win-x64'
$releaseDir = if ($ReleaseDirectory) { [IO.Path]::GetFullPath($ReleaseDirectory) } else { Join-Path $projectRoot 'artifacts\release' }
if (-not $InnoCompiler) {
    $InnoCompiler = Join-Path $projectRoot 'artifacts\tools\InnoSetup\ISCC.exe'
    if (-not (Test-Path -LiteralPath $InnoCompiler)) {
        $compilerCommand = Get-Command ISCC.exe -ErrorAction SilentlyContinue
        if ($compilerCommand) { $InnoCompiler = $compilerCommand.Source }
    }
}
if (-not (Test-Path -LiteralPath $InnoCompiler)) { throw 'Provide -InnoCompiler with the path to Inno Setup 7 ISCC.exe.' }

# Only clean this build's fixed staging folder inside the workspace.
if (Test-Path -LiteralPath $publishDir) {
    $resolvedPublish = (Resolve-Path -LiteralPath $publishDir).Path
    $expectedPublish = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts\publish\win-x64'))
    if ($resolvedPublish -ne $expectedPublish) { throw 'Unexpected publish path; refusing to clean.' }
    Remove-Item -LiteralPath $resolvedPublish -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $publishDir,$releaseDir | Out-Null
$publishArguments = @('publish', (Join-Path $projectRoot 'CatVideo.csproj'), '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-p:PublishSingleFile=false', '-p:PublishTrimmed=false', '-p:DebugType=None', "-p:Version=$Version", '-o', $publishDir)
if ($RestoreSource) { $publishArguments += "-p:RestoreSources=$RestoreSource" }
if ($PackagesPath) { $publishArguments += "-p:RestorePackagesPath=$PackagesPath" }
& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) { throw 'Application compilation failed.' }

$runtimeConfig = Get-Content -LiteralPath (Join-Path $publishDir 'CatVideo.runtimeconfig.json') -Raw | ConvertFrom-Json
if ($runtimeConfig.runtimeOptions.framework -or $runtimeConfig.runtimeOptions.frameworks) { throw 'Release unexpectedly depends on a separately installed .NET runtime.' }
foreach ($requiredFile in @('CatVideo.exe', 'coreclr.dll', 'hostfxr.dll', 'PresentationFramework.dll', 'System.Private.CoreLib.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDir $requiredFile))) { throw "Missing bundled file: $requiredFile" }
}

Copy-Item -LiteralPath (Join-Path $projectRoot 'packaging\Release-Notes.md') -Destination (Join-Path $releaseDir 'Release-Notes.md') -Force
# Preserve the application's license and the notices from the actual bundled runtime packages.
foreach ($document in @('LICENSE', 'THIRD-PARTY-NOTICES.md', 'PRIVACY.md')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $document) -Destination $publishDir -Force
}
$packageRoot = if ($PackagesPath) { [IO.Path]::GetFullPath($PackagesPath) } elseif ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path ([Environment]::GetFolderPath('UserProfile')) '.nuget\packages' }
$coreVersion = ($runtimeConfig.runtimeOptions.includedFrameworks | Where-Object name -eq 'Microsoft.NETCore.App').version
$desktopVersion = ($runtimeConfig.runtimeOptions.includedFrameworks | Where-Object name -eq 'Microsoft.WindowsDesktop.App').version
if (-not $coreVersion -or -not $desktopVersion) { throw 'Cannot identify bundled runtime versions.' }
$noticeFiles = @(
    @{ Source = Join-Path $packageRoot "microsoft.netcore.app.runtime.win-x64\$coreVersion\LICENSE.TXT"; Folder = 'dotnet'; Name = 'LICENSE.TXT' },
    @{ Source = Join-Path $packageRoot "microsoft.netcore.app.runtime.win-x64\$coreVersion\THIRD-PARTY-NOTICES.TXT"; Folder = 'dotnet'; Name = 'THIRD-PARTY-NOTICES.TXT' },
    @{ Source = Join-Path $packageRoot "microsoft.windowsdesktop.app.runtime.win-x64\$desktopVersion\LICENSE"; Folder = 'windowsdesktop'; Name = 'LICENSE.TXT' },
    @{ Source = Join-Path (Split-Path $InnoCompiler -Parent) 'license.txt'; Folder = 'innosetup'; Name = 'LICENSE.TXT' }
)
foreach ($notice in $noticeFiles) {
    if (-not (Test-Path -LiteralPath $notice.Source)) { throw "Missing third-party license: $($notice.Source)" }
    $noticeDirectory = Join-Path $publishDir "licenses\$($notice.Folder)"
    New-Item -ItemType Directory -Force -Path $noticeDirectory | Out-Null
    Copy-Item -LiteralPath $notice.Source -Destination (Join-Path $noticeDirectory $notice.Name) -Force
}

$sourceCommit = $null
$sourceDirty = $null
if (Get-Command git -ErrorAction SilentlyContinue) {
    $sourceCommit = & git -c "safe.directory=$projectRoot" -C $projectRoot rev-parse HEAD 2>$null
    if ($LASTEXITCODE -eq 0) {
        $sourceDirty = [bool](& git -c "safe.directory=$projectRoot" -C $projectRoot status --porcelain)
    } else { $sourceCommit = $null }
}
$sdkVersion = & dotnet --version
if ($LASTEXITCODE -ne 0) { throw 'Cannot identify build SDK.' }
$compilerBanner = & $InnoCompiler '/?'
$compilerVersionMatch = [regex]::Match(($compilerBanner -join "`n"), 'Compiler engine version:\s*Inno Setup ([\d.]+)')
if (-not $compilerVersionMatch.Success) { throw 'Cannot identify Inno Setup compiler engine version.' }
$buildInfo = [ordered]@{
    Product = 'Cat螢幕轉換器'; Publisher = 'QxQ'; Version = $Version; License = 'MIT'
    Repository = 'https://github.com/LeeWeiCheng/CatScreenConverter'
    Commit = $sourceCommit; WorkingTreeDirty = $sourceDirty
    Sdk = $sdkVersion; CoreRuntime = $coreVersion; DesktopRuntime = $desktopVersion
    InnoCompilerVersion = $compilerVersionMatch.Groups[1].Value
    InnoCompilerSHA256 = (Get-FileHash -LiteralPath $InnoCompiler -Algorithm SHA256).Hash.ToLowerInvariant()
    GitHubRunUrl = if ($env:GITHUB_RUN_ID) { "$env:GITHUB_SERVER_URL/$env:GITHUB_REPOSITORY/actions/runs/$env:GITHUB_RUN_ID" } else { $null }
    Signature = 'unsigned'; RuntimeTestsExecuted = $false
}
$buildInfo | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $publishDir 'Build-Info.json') -Encoding utf8
Copy-Item -LiteralPath (Join-Path $publishDir 'Build-Info.json') -Destination $releaseDir -Force
& $InnoCompiler "/DAppVersion=$Version" "/DPublishDir=$publishDir" "/DReleaseDir=$releaseDir" (Join-Path $PSScriptRoot 'CatScreen.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$installer = Join-Path $releaseDir "QxQ-CatScreenConverter-Setup-$Version-win-x64.exe"
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($installer))" | Set-Content -LiteralPath (Join-Path $releaseDir 'SHA256SUMS.txt') -Encoding ascii
Write-Output "Release compiled: $installer"
Write-Output 'Installation and launch testing have not been run.'
