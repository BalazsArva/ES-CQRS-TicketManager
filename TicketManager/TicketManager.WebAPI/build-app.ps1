param($BuildConfiguration = "Release")

if (Test-Path -Path ".\dist") {
    Remove-Item .\dist -Force -Recurse
}

$composeArgs = @()
$composeArgs += "publish"
$composeArgs += "--framework"
$composeArgs += "netcoreapp2.2"
$composeArgs += "--configuration"
$composeArgs += $BuildConfiguration
$composeArgs += "--output"
$composeArgs += "dist"

Invoke-Expression -Command "dotnet $composeArgs"