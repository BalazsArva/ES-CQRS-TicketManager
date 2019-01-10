param(
    $RavenDbCoresPerNode = 3,
    $BuildConfiguration = "Release",
    $MSSQL_DBHOST = "mssql",
    $MSSQL_DBUSERID = "sa",
    $MSSQL_DBPORT = 1433,
    $MSSQL_DBNAME = "CQRSTicketManager",
    $MSSQL_SA_PASSWORD,
    [switch]$MigrateDatabase,
    [switch]$BuildOnly,
    [switch]$DontSetupCluster)

$ErrorActionPreference = 'Stop'

# Delete dist folder and rebuild app
& .\build-app.ps1 -BuildConfiguration $BuildConfiguration

if ([string]::IsNullOrEmpty($MSSQL_SA_PASSWORD)) {
    $MSSQL_SA_PASSWORD = read-host "Password for the SA MSSQL user"
}

$env:MSSQL_DBHOST="$MSSQL_DBHOST"
$env:MSSQL_DBUSERID="$MSSQL_DBUSERID"
$env:MSSQL_DBPORT="$MSSQL_DBPORT"
$env:MSSQL_DBNAME="$MSSQL_DBNAME"
$env:MSSQL_SA_PASSWORD="$MSSQL_SA_PASSWORD"

$composeCommand = "docker-compose"
$composeArgs = @()

if ($BuildOnly) {
    $composeArgs += "build";
} else {
    $composeArgs += "up";

    if ($MigrateDatabase) {
        $env:MSSQL_DBMIGRATE = "true";
    }
}

$composeArgs += "--force-recreate"
$composeArgs += "-d"

Invoke-Expression -Command "$composeCommand $composeArgs"

& .\run-ravendb.ps1 -CoresPerNode $RavenDbCoresPerNode