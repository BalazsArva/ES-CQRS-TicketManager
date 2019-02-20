$OriginalLocation = Get-Location

Set-Location ..\TicketManager

$ReceiverFolders1 = Get-ChildItem -Directory TicketManager.Receivers.*
$ReceiverFolders2 = Get-ChildItem -Directory TicketManager.Notifications.*

$ReceiverFolders = $ReceiverFolders1 + $ReceiverFolders2

Write-Host $ReceiverFolders

foreach ($folder in $ReceiverFolders | Select-Object) {
    Write-Host $folder

    $TargetDirectory = "$folder\Properties"
    $TargetFile = "$TargetDirectory\launchSettings.json"

    if (!(Test-Path -Path $TargetDirectory)) {
        New-Item -ItemType directory -Path $TargetDirectory
    }

    if (!(Test-Path -Path $TargetFile)) {
        New-Item -ItemType file -Path $TargetFile
    }

    $ProjectName = Split-Path -Path $TargetDirectory\.. -Leaf

    $Content = @{}
    $Content["profiles"] = @{}
    $Content["profiles"][$ProjectName] = @{}
    $Content["profiles"][$ProjectName]["commandName"] = "Project"
    $Content["profiles"][$ProjectName]["environmentVariables"] = @{}
    $Content["profiles"][$ProjectName]["environmentVariables"]["RECEIVER_ENVIRONMENT"] = "Development"

    $Content | ConvertTo-Json -Depth 3 | Out-File -FilePath $TargetFile
}

Set-Location $OriginalLocation