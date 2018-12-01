using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
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
                .AddTransient<IValidator<AddTicketLinksCommand>, AddTicketLinksCommandValidator>()
                .AddTransient<IValidator<AddTicketTagsCommand>, AddTicketTagsCommandValidator>()
                .AddTransient<IValidator<AssignTicketCommand>, AssignTicketCommandValidator>()
                .AddTransient<IValidator<ChangeTicketPriorityCommand>, ChangeTicketPriorityCommandValidator>()
                .AddTransient<IValidator<ChangeTicketStatusCommand>, ChangeTicketStatusCommandValidator>()
                .AddTransient<IValidator<ChangeTicketTypeCommand>, ChangeTicketTypeCommandValidator>()
                .AddTransient<IValidator<CreateTicketCommand>, CreateTicketCommandValidator>()
                .AddTransient<IValidator<EditTicketCommentCommand>, EditTicketCommentCommandValidator>()
                .AddTransient<IValidator<EditTicketTitleCommand>, EditTicketTitleCommandValidator>()
                .AddTransient<IValidator<EditTicketDescriptionCommand>, EditTicketDescriptionCommandValidator>()
                .AddTransient<IValidator<PostCommentToTicketCommand>, PostCommentToTicketCommandValidator>()
                .AddTransient<IValidator<RemoveTicketLinksCommand>, RemoveTicketLinksCommandValidator>()
                .AddTransient<IValidator<RemoveTicketTagsCommand>, RemoveTicketTagsCommandValidator>()
                .AddTransient<IValidator<UpdateTicketCommand>, UpdateTicketCommandValidator>()
                .AddTransient<IValidator<TicketLinkDTO>, TicketLinkValidator>();
        }
    }
}