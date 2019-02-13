# How to use

In development environment, the ServiceBus connection string is retrieved using the user secrets feature. The configuration value for the Service Bus connection string is retrieved by key `ServiceBus:ConnectionString`, so a secret with this key must exist.

To set that secret, open PowerShell in the directory of this project and run the following command:

    dotnet user-secrets set ServiceBus:ConnectionString <your connection string>

All apps in this solution which use the user secrets feature are configured to use the same secret collection identifier (which can be found in the .csproj file in the `<UserSecretsId>` element) so it is enough to set it from the directory of either of these projects and all apps will be able to access it.

To determine whether the app is running in development environment, an environment variable called `RECEIVER_ENVIRONMENT` must be set to `Development`. With Visual Studio, this can be set by opening the project properties page, and under the Debug tab, creating this entry. With Visual Studio Code or other tools, a `Properties/launchsettings.json` file must exist (which is gitignored, so a `git clean -xfd` will remove it) and should contain the following content:

    {
      "profiles": {
        "TicketManager.Receivers.TicketAssigned": {
          "commandName": "Project",
          "environmentVariables": {
            "RECEIVER_ENVIRONMENT": "Development"
          }
        }
      }
    }

