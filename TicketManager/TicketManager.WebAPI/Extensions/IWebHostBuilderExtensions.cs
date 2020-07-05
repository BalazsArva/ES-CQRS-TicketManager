using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.Common.StartupTasks.Abstractions;

namespace TicketManager.WebAPI.Extensions
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseStartupTask<TStartupTask>(
            this IWebHostBuilder webHostBuilder,
            Func<IConfiguration, bool> executionCondition = null)
            where TStartupTask : class, IApplicationStartupTask
        {
            if (webHostBuilder is null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            return webHostBuilder.ConfigureServices((ctx, services) =>
            {
                if (executionCondition is null || executionCondition(ctx.Configuration))
                {
                    services.AddSingleton<IApplicationStartupTask, TStartupTask>();
                }
            });
        }
    }
}