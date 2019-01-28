using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
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
            services.AddRavenDb(Configuration);
            services.AddEventsContext(Configuration);

            services.AddValidators();
            services.AddEventAggregators();
            services.AddApplicationUtilities();
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