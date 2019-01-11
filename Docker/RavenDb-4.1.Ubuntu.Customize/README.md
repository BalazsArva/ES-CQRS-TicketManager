# Description

This is a customization of the RavenDB 4.1 Ubuntu image. This image builds on top of the official [RavenDB 4.1 Ubuntu image](https://github.com/ravendb/ravendb/blob/v4.1/docker/ravendb-ubuntu/Dockerfile) and its current purpose is to install the `curl` command line tool to the OS which is necessary for [automated cluster setup](https://github.com/ravendb/ravendb/tree/v4.1/docker/compose/linux-cluster) as the official image seems not to contain this tool despite being used in the [provided cluster setup script](https://github.com/ravendb/ravendb/blob/v4.1/docker/compose/linux-cluster/run.ps1).

# How to use

The customized image can be built individually by running the `build.ps1` script found in this folder.

Additionally, the image can be built as part of the entire build toolchain, by executing the `run.ps1` script found in `<repo root>\TicketManager\TicketManager.WebAPI` and setting the `-BuildRavenDbImage` switch on:

    TicketManager\TicketManager.WebAPI> .\run.ps1 -BuildRavenDbImage

