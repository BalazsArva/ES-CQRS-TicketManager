# Based on https://github.com/ravendb/ravendb/blob/v4.0/docker/compose/linux-cluster/run.ps1
param($CoresPerNode = 3)

$nodes = @(
    @{ Tag = "A"; Url = "http://ravendb1:8080" },
    @{ Tag = "B"; Url = "http://ravendb2:8080" },
    @{ Tag = "C"; Url = "http://ravendb3:8080" }
);

function AddNodeToCluster() {
    param($FirstNodeUrl, $OtherNodeUrl, $AssignedCores = 1)

    $WaitSeconds = 5
    $otherNodeUrlEncoded = $OtherNodeUrl

    Write-Host -ForegroundColor Yellow "Add node $otherNodeUrlEncoded to cluster"

    $uri = "$($FirstNodeUrl)/admin/cluster/node?url=$($otherNodeUrlEncoded)&assignedCores=$AssignedCores"
    $curlCmd = "curl -L -X PUT '$uri' -d ''"
    docker exec -it ravendb1 bash -c "$curlCmd"

    Write-Host -ForegroundColor DarkGray "Waiting $WaitSeconds seconds for cluster to stabilize..."
    Write-Host

    Start-Sleep -Seconds $WaitSeconds
}

function SetNodeCores() {
    param($ClusterMasterUrl, $NodeTag, $Cores)

    write-host -ForegroundColor Yellow "Reassign cores on $NodeTag to $CoresPerNode"

    $uri = "$($ClusterMasterUrl)/admin/license/set-limit?nodeTag=$NodeTag&newAssignedCores=$Cores"
    $curlCmd = "curl -L -X POST '$uri' -d ''"
    docker exec -it ravendb1 bash -c "$curlCmd"

    # Write-Host -ForegroundColor DarkGray "Waiting 10 seconds for cluster to stabilize..."
    Write-Host

    # Start-Sleep -Seconds 10
}

Start-Sleep -Seconds 10

$firstNodeIp = $nodes[0].Url
$nodeAcoresReassigned = $false

Write-Host
Write-Host -ForegroundColor Magenta "Setting up RavenDb..."

foreach ($node in $nodes | Select-Object -Skip 1) {
    AddNodeToCluster -FirstNodeUrl $firstNodeIp -OtherNodeUrl $node.Url

    if ($nodeAcoresReassigned -eq $false) {
        SetNodeCores -ClusterMasterUrl $firstNodeIp -NodeTag $nodes[0].Tag -Cores $CoresPerNode

        $nodeAcoresReassigned = $true
    }

    SetNodeCores -ClusterMasterUrl $firstNodeIp -NodeTag $node.Tag -Cores $CoresPerNode
}

Write-Host -ForegroundColor Magenta "RavenDb setup complete"
Write-Host