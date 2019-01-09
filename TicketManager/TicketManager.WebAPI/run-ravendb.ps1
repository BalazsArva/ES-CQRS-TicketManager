# Based on https://github.com/ravendb/ravendb/blob/v4.0/docker/compose/linux-cluster/run.ps1
param([switch]$DontSetupCluster)

if ($DontSetupCluster) {
    exit 0
}

$nodes = @(
    "http://ravendb1:8080",
    "http://ravendb2:8080",
    "http://ravendb3:8080"
);

function EnsureCurlInstalled() {
    docker exec -it ravendb1 bash -c "apt-get update && apt-get install -y && apt-get install -y --no-install-recommends curl"
}

function AddNodeToCluster() {
    param($FirstNodeUrl, $OtherNodeUrl, $AssignedCores = 1)

    $otherNodeUrlEncoded = $OtherNodeUrl
    $uri = "$($FirstNodeUrl)/admin/cluster/node?url=$($otherNodeUrlEncoded)&assignedCores=$AssignedCores"
    $curlCmd = "curl -L -X PUT '$uri' -d ''"
    docker exec -it ravendb1 bash -c "$curlCmd"
    Write-Host
    Start-Sleep -Seconds 10
}

Start-Sleep -Seconds 10 

$firstNodeIp = $nodes[0]
$nodeAcoresReassigned = $false

EnsureCurlInstalled

foreach ($node in $nodes | Select-Object -Skip 1) {
    write-Host "Add node $node to cluster";
    AddNodeToCluster -FirstNodeUrl $firstNodeIp -OtherNodeUrl $node

    if ($nodeAcoresReassigned -eq $false) {
        write-host "Reassign cores on A to 1"
        $uri = "$($firstNodeIp)/admin/license/set-limit?nodeTag=A&newAssignedCores=1"
        $curlCmd = "curl -L -X POST '$uri' -d ''"
        docker exec -it ravendb1 bash -c "$curlCmd"
    }
}

write-host "These run on Hyper-V, so they are available under one IP - usually 10.0.75.2, so:"
write-host "ravendb1 10.0.75.2:8081"
write-host "ravendb2 10.0.75.2:8082"
write-host "ravendb3 10.0.75.2:8083"