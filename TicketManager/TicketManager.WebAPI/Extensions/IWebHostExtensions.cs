using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.Common.StartupTasks.Abstractions;
using TicketManager.Common.Utils;

namespace TicketManager.WebAPI.Extensions
{
    public static class IWebHostExtensions
    {
        public static async Task ExecuteStartupTasksAsync(this IWebHost webHost)
        {
            Throw.IfNull(nameof(webHost), webHost);

            var startupTasks = webHost.Services.GetServices<IApplicationStartupTask>();

            foreach (var startupTask in startupTasks)
            {
                await startupTask.ExecuteAsync(default);
            }
        }
    }
}