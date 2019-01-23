param(
    $RavenDbCoresPerNode = 3,
    $BuildConfiguration = "Release",
    $Scale = 1,
    $MSSQL_DBHOST = "mssql",
    $MSSQL_DBUSERID = "sa",
    $MSSQL_DBPORT = 1433,
    $MSSQL_DBNAME = "CQRSTicketManager",
    $MSSQL_SA_PASSWORD,
    [switch]$RecreateVolumes,
    [switch]$BuildRavenDbImage,
    [switch]$MigrateDatabase,
    [switch]$SetupRavenDbCluster,
    [switch]$BuildOnly)

$ErrorActionPreference = 'Stop'

& .\Utilities\restore-packages.ps1
& .\build-app.ps1 -BuildConfiguration $BuildConfiguration

if ($BuildRavenDbImage) {
    Write-Host -ForegroundColor Magenta "Building customized RavenDB image..."
    Write-Host -ForegroundColor White

    $OriginalLocation = (Get-Location).Path
    Set-Location -Path "$OriginalLocation\RavenDb-4.1.Ubuntu.Customize"
    
    .\build.ps1

    Set-Location -Path $OriginalLocation

    Write-Host -ForegroundColor Magenta "Finished building customized RavenDB image."
    Write-Host -ForegroundColor White
}
function Decompose() {
    $Command = "docker-compose"
    $CommandArgs = @()

    $CommandArgs += "down"
    if ($RecreateVolumes) {
        $CommandArgs += "-v"
    }

    Invoke-Expression -Command "$Command $CommandArgs"
}

function Compose() {
    $composeCommand = "docker-compose"
    $composeArgs = @()

    if ($BuildOnly) {
        $composeArgs += "build"
        $composeArgs += "--force-recreate"
        $composeArgs += "-d"
    
        Invoke-Expression -Command "$composeCommand $composeArgs"
    } else {
        $composeArgs += "up"
        $composeArgs += "--build"
        $composeArgs += "--force-recreate"
        $composeArgs += "-d"
    
        if ($MigrateDatabase) {
            $env:MSSQL_DBMIGRATE = "true";
        }
    
        Invoke-Expression -Command "$composeCommand $composeArgs"
    
        if ($SetupRavenDbCluster) {
            & .\setup-ravendb-cluster.ps1 -CoresPerNode $RavenDbCoresPerNode
        }
    }
}

if ([string]::IsNullOrEmpty($MSSQL_SA_PASSWORD)) {
    $MSSQL_SA_PASSWORD = read-host "Password for the SA MSSQL user"
}

$env:MSSQL_DBHOST = "$MSSQL_DBHOST"
$env:MSSQL_DBUSERID = "$MSSQL_DBUSERID"
$env:MSSQL_DBPORT = "$MSSQL_DBPORT"
$env:MSSQL_DBNAME = "$MSSQL_DBNAME"
$env:MSSQL_SA_PASSWORD = "$MSSQL_SA_PASSWORD"

Decompose
Compose