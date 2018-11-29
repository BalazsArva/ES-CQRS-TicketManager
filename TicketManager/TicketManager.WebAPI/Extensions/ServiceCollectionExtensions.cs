using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.Validation.CommandValidators;

namespace TicketManager.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRavenDb(this IServiceCollection services, IConfiguration configuration)
        {
            var ravenDbUrls = configuration.GetSection("DataAccess:RavenDb:Urls").Get<string[]>();
            var ravenDbDatabase = configuration["DataAccess:RavenDb:Database"];

            services.AddSingleton(new DocumentStore
            {
                Urls = ravenDbUrls,
                Database = ravenDbDatabase
            }.Initialize());

            return services;
        }

        public static IServiceCollection AddEventsContext(this IServiceCollection services, IConfiguration Configuration)
        {
            var sqlConnectionString = Configuration["DataAccess:SQL"];

            var options = new DbContextOptionsBuilder<EventsContext>()
                .UseSqlServer(sqlConnectionString)
                .Options;

            services.AddSingleton(options);
            services.AddDbContext<EventsContext>(opts => opts.UseSqlServer(sqlConnectionString));
            services.AddSingleton<IEventsContextFactory>(svcProvider =>
            {
                return new EventsContextFactory(options);
            });

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            return services
                .AddTransient<AddTicketLinksCommandValidator>()
                .AddTransient<AddTicketTagCommandValidator>()
                .AddTransient<AssignTicketCommandValidator>()
                .AddTransient<ChangeTicketStatusCommandValidator>()
                .AddTransient<CreateTicketCommandValidator>()
                .AddTransient<EditTicketCommentCommandValidator>()
                .AddTransient<EditTicketDetailsCommandValidator>()
                .AddTransient<PostCommentToTicketCommandValidator>()
                .AddTransient<RemoveTicketLinksCommandValidator>()
                .AddTransient<RemoveTicketTagCommandValidator>()
                .AddTransient<UpdateTicketCommandValidator>()
                .AddTransient<TicketLinkValidator>();
        }
    }
}