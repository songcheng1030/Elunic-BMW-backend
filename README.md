# Quickstart

Overview of how to develop the services with WSL2 or Linux environment.

## Using docker compose

Run `bash shell.sh` to drop into the docker shell. It provides you with dotnet cli and starts up a mssql database for development purpose. Once in the shell, the required env variables should be set, run `bash install_deps.sh` to run though the services and install the required packages automatically. To startup the services in parallel use `bash dev.sh`.

If you need dotnet ef (dotnet entity framework cli), you can navigate into a service and use `dotnet tool restore` (this has to be run once per service every time you startup the shell). You can then use the cli to via `dotnet tool run dotnet-ef <COMMAND>` see (https://docs.microsoft.com/de-de/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli).

To build a service, navigate into the folder and run `dotnet build`. 

`dotnet watch run` can be used to debug a single service.