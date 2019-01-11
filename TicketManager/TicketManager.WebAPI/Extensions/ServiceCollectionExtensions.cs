using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.Services.EventAggregators;
using TicketManager.WebAPI.Validation.CommandValidators;
using TicketManager.WebAPI.Validation.QueryValidators;

namespace TicketManager.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventsContext(this IServiceCollection services, IConfiguration Configuration)
        {
            // TODO: Maybe should default to localhost and local settings (integrated security, etc.)
            var dbHost = Configuration["DBHOST"] ?? "mssql";
            var userId = Configuration["DBUSERID"] ?? "sa";
            var password = Configuration["SA_PASSWORD"];
            var dbPort = Configuration["DBPORT"] ?? "1433";
            var database = Configuration["DBNAME"] ?? "CQRSTicketManager";

            var sqlConnectionString = $"Data Source={dbHost},{dbPort};Initial Catalog={database};User Id={userId};Password={password}";

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
            services
                .AddSingleton<IValidator<AddTicketLinksCommand>, AddTicketLinksCommandValidator>()
                .AddSingleton<IValidator<AddTicketTagsCommand>, AddTicketTagsCommandValidator>()
                .AddSingleton<IValidator<AssignTicketCommand>, AssignTicketCommandValidator>()
                .AddSingleton<IValidator<CancelTicketInvolvementCommand>, CancelTicketInvolvementCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketPriorityCommand>, ChangeTicketPriorityCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketStatusCommand>, ChangeTicketStatusCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketTypeCommand>, ChangeTicketTypeCommandValidator>()
                .AddSingleton<IValidator<CreateTicketCommand>, CreateTicketCommandValidator>()
                .AddSingleton<IValidator<EditTicketCommentCommand>, EditTicketCommentCommandValidator>()
                .AddSingleton<IValidator<EditTicketTitleCommand>, EditTicketTitleCommandValidator>()
                .AddSingleton<IValidator<EditTicketDescriptionCommand>, EditTicketDescriptionCommandValidator>()
                .AddSingleton<IValidator<PostCommentToTicketCommand>, PostCommentToTicketCommandValidator>()
                .AddSingleton<IValidator<RemoveTicketLinksCommand>, RemoveTicketLinksCommandValidator>()
                .AddSingleton<IValidator<RemoveTicketTagsCommand>, RemoveTicketTagsCommandValidator>()
                .AddSingleton<IValidator<UpdateTicketCommand>, UpdateTicketCommandValidator>()
                .AddSingleton<TicketLinkValidator_AddLinks>()
                .AddSingleton<TicketLinkValidator_CreateInitialLinks>()
                .AddSingleton<TicketLinkValidator_UpdateLinks>();

            services
                .AddSingleton<IValidator<SearchTicketsQueryRequest>, SearchTicketsQueryRequestValidator>()
                .AddSingleton<IValidator<GetTicketHistoryQueryRequest>, GetTicketHistoryQueryRequestValidator>();

            return services;
        }

        public static IServiceCollection AddEventAggregators(this IServiceCollection services)
        {
            services
                .AddTransient<IEventAggregator<Assignment>, TicketAssignedEventAggregator>()
                .AddTransient<IEventAggregator<TicketInvolvement>, TicketUserInvolvementEventAggregator>()
                .AddTransient<IEventAggregator<TicketDescription>, TicketDescriptionChangedEventAggregator>()
                .AddTransient<IEventAggregator<Links>, TicketLinksChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketPriority>, TicketPriorityChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketStatus>, TicketStatusChangedEventAggregator>()
                .AddTransient<IEventAggregator<Tags>, TicketTagsChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketTitle>, TicketTitleChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketType>, TicketTypeChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketInvolvement>, TicketUserInvolvementEventAggregator>();

            return services;
        }
    }
}