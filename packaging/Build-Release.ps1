param(
    [string]$Version = '1.0.0',
    [string]$InnoCompiler,
    [string]$RestoreSource,
    [string]$PackagesPath
)

$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must have the form 1.0.0.' }
$projectRoot = Split-Path $PSScriptRoot -Parent
$publishDir = Join-Path $projectRoot 'artifacts\publish\win-x64'
$releaseDir = Join-Path $projectRoot 'artifacts\release'
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
& $InnoCompiler "/DAppVersion=$Version" "/DPublishDir=$publishDir" "/DReleaseDir=$releaseDir" (Join-Path $PSScriptRoot 'CatScreen.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$installer = Join-Path $releaseDir "QxQ-CatScreenConverter-Setup-$Version-win-x64.exe"
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($installer))" | Set-Content -LiteralPath (Join-Path $releaseDir 'SHA256SUMS.txt') -Encoding ascii
Write-Output "Release compiled: $installer"
Write-Output 'Installation and launch testing have not been run.'
