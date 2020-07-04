using CorrelationId;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.BusinessServices.EventAggregators.Extensions;
using TicketManager.Common.Http;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.WebAPI.Extensions;
using TicketManager.WebAPI.Filters;

namespace TicketManager.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelationId();

            services.AddRavenDb(Configuration);
            services.AddEventsContext(Configuration);

            services
                .AddValidators()
                .AddStartupTasks();

            // TODO: Delete (project reference as well) when everything is moved to the separate query store synchronizer apps.
            services.AddEventAggregators();
            services.AddApplicationServices(Configuration, HostingEnvironment);
            services.AddMediatR(typeof(Startup).Assembly);

            services
                .AddMvc(opts =>
                {
                    opts.Filters.Add<ValidationExceptionFilterAttribute>();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "TicketManager",
                    Version = "v1",
                    Description = "The API for the CQRS TicketManager application",
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCorrelationId(new CorrelationIdOptions
            {
                Header = CustomRequestHeaders.CorrelationId,
                IncludeInResponse = true,
                UseGuidForCorrelationId = true,

                // Need to set to false due to a bug in Asp.Net Core which causes the HttpContext to become null if the TraceIdentifier is changed.
                // See https://github.com/aspnet/AspNetCore/issues/5144 and https://github.com/stevejgordon/CorrelationId#known-issue-with-aspnet-core-220
                UpdateTraceIdentifier = false
            });

            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketManager API v1");
            });

            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}