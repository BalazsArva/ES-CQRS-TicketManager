using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TicketManager.WebAPI.StartupTasks.Abstractions;

namespace TicketManager.WebAPI
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var app = CreateWebHostBuilder(args).Build();

            foreach (var startupTask in app.Services.GetServices<IApplicationStartupTask>())
            {
                await startupTask.ExecuteAsync(default);
            }

            app.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cfg => cfg.AddEnvironmentVariables("MSSQL_"))
                .UseStartup<Startup>();
        }
    }
}