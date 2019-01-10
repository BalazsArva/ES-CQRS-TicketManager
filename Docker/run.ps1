param(
    $RavenDbCoresPerNode = 3,
    $BuildConfiguration = "Release",
    $MSSQL_DBHOST = "mssql",
    $MSSQL_DBUSERID = "sa",
    $MSSQL_DBPORT = 1433,
    $MSSQL_DBNAME = "CQRSTicketManager",
    $MSSQL_SA_PASSWORD,
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

    cd .\RavenDb-4.1.Ubuntu.Customize
    .\build.ps1
    cd ..

    Write-Host -ForegroundColor Magenta "Finished building customized RavenDB image."
    Write-Host -ForegroundColor White
}

if ([string]::IsNullOrEmpty($MSSQL_SA_PASSWORD)) {
    $MSSQL_SA_PASSWORD = read-host "Password for the SA MSSQL user"
}

$env:MSSQL_DBHOST = "$MSSQL_DBHOST"
$env:MSSQL_DBUSERID = "$MSSQL_DBUSERID"
$env:MSSQL_DBPORT = "$MSSQL_DBPORT"
$env:MSSQL_DBNAME = "$MSSQL_DBNAME"
$env:MSSQL_SA_PASSWORD = "$MSSQL_SA_PASSWORD"

$composeCommand = "docker-compose"
$composeArgs = @()

if ($BuildOnly) {
    $composeArgs += "build";
    $composeArgs += "--force-recreate"
    $composeArgs += "-d"

    Invoke-Expression -Command "$composeCommand $composeArgs"
} else {
    $composeArgs += "up";
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