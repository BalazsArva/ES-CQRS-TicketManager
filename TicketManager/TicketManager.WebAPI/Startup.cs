using CorrelationId;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using TicketManager.Common.Http;
using TicketManager.WebAPI.Extensions;
using TicketManager.WebAPI.Filters;

namespace TicketManager.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelationId();

            services.AddRavenDb(Configuration);
            services.AddEventsContext(Configuration);

            services.AddValidators();
            services.AddEventAggregators();
            services.AddApplicationServices();
            services.AddMediatR(typeof(Startup).Assembly);

            services
                .AddMvc(opts =>
                {
                    opts.Filters.Add<ValidationExceptionFilterAttribute>();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "TicketManager", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketManager API v1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();

            if (bool.TryParse(Configuration["DBMIGRATE"], out var migrate) && migrate)
            {
                app.MigrateDatabase();
            }
        }
    }
}