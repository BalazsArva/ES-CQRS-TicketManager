$WaitForItPath = "$PSScriptRoot\node_modules\wait-for-it.sh\bin\wait-for-it"

if (Test-Path -Path $WaitForItPath) {
    Write-Host -ForegroundColor Green "wait-for-it.sh package found, skipping package install."
} else {
    Write-Host -ForegroundColor Magenta "wait-for-it.sh package not found, attempting to install it..."

    $PreviousLocation = Get-Location

    Set-Location -Path $PSScriptRoot
    npm install

    Set-Location -Path $PreviousLocation.Path
}

Write-Host -ForegroundColor White ""