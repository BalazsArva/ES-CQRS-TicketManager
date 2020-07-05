using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using TicketManager.DataAccess.Documents.StartupTasks;
using TicketManager.DataAccess.Events.StartupTasks;
using TicketManager.WebAPI.Extensions;

namespace TicketManager.WebAPI
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var app = CreateWebHostBuilder(args).Build();

            await app.ExecuteStartupTasksAsync();
            await app.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cfg => cfg.AddEnvironmentVariables("MSSQL_"))
                .UseStartup<Startup>()
                .UseStartupTask<SetupDocumentsDatabase>()
                .UseStartupTask<MigrateEventsDatabase>(cfg => bool.TryParse(cfg["DBMIGRATE"], out var migrate) && migrate);
        }
    }
}