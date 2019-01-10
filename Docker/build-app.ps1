param($BuildConfiguration = "Release")

$projectPath = "../TicketManager/TicketManager.WebAPI"
$outputPath = "./dist"

if (Test-Path -Path $outputPath) {
    Remove-Item $outputPath -Force -Recurse
}

$dotnetPublishArgs = @()
$dotnetPublishArgs += "publish"
$dotnetPublishArgs += $projectPath
$dotnetPublishArgs += "--framework"
$dotnetPublishArgs += "netcoreapp2.2"
$dotnetPublishArgs += "--configuration"
$dotnetPublishArgs += $BuildConfiguration
$dotnetPublishArgs += "--output"

# This must be relative to the csproj, not the current context (Docker/)
$dotnetPublishArgs += "../../Docker/dist"

$dotnetRestoreArgs = @()
$dotnetRestoreArgs += "restore"
$dotnetRestoreArgs += $projectPath

Invoke-Expression -Command "dotnet $dotnetRestoreArgs"
Invoke-Expression -Command "dotnet $dotnetPublishArgs"