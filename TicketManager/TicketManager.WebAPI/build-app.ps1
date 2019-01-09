param($BuildConfiguration = "Release")

if (Test-Path -Path ".\dist") {
    Remove-Item .\dist -Force -Recurse
}

$dotnetPublishArgs = @()
$dotnetPublishArgs += "publish"
$dotnetPublishArgs += "--framework"
$dotnetPublishArgs += "netcoreapp2.2"
$dotnetPublishArgs += "--configuration"
$dotnetPublishArgs += $BuildConfiguration
$dotnetPublishArgs += "--output"
$dotnetPublishArgs += "dist"

$dotnetRestoreArgs = @()
$dotnetRestoreArgs += "restore"

Invoke-Expression -Command "dotnet $dotnetRestoreArgs"
Invoke-Expression -Command "dotnet $dotnetPublishArgs"