using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TicketManager.DataAccess.Documents.Utilities;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.MessageClients;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.Services.Providers;
using TicketManager.WebAPI.Validation.CommandValidators;
using TicketManager.WebAPI.Validation.QueryValidators;

namespace TicketManager.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services
                .AddSingleton<IValidator<AddTicketLinksCommand>, AddTicketLinksCommandValidator>()
                .AddSingleton<IValidator<AddTicketTagsCommand>, AddTicketTagsCommandValidator>()
                .AddSingleton<IValidator<AssignTicketCommand>, AssignTicketCommandValidator>()
                .AddSingleton<IValidator<CancelTicketInvolvementCommand>, CancelTicketInvolvementCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketPriorityCommand>, ChangeTicketPriorityCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketStatusCommand>, ChangeTicketStatusCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketStoryPointsCommand>, ChangeTicketStoryPointsCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketTypeCommand>, ChangeTicketTypeCommandValidator>()
                .AddSingleton<IValidator<CreateTicketCommand>, CreateTicketCommandValidator>()
                .AddSingleton<IValidator<EditTicketCommentCommand>, EditTicketCommentCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketTitleCommand>, ChangeTicketTitleCommandValidator>()
                .AddSingleton<IValidator<ChangeTicketDescriptionCommand>, ChangeTicketDescriptionCommandValidator>()
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

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            var sbTopic = "ticketevents";

            // Suffix topic name with machine name for development so multiple workstations (e.g. my home and
            // workplace machine) don't mess with each other's messages.
            if (hostingEnvironment.IsDevelopment())
            {
                sbTopic = $"{sbTopic}.{Environment.MachineName}";
            }

            var topicConfiguration = new ServiceBusTopicConfiguration
            {
                // The ConnectionString is stored as a user secret. Set it on the machine before using by navigating to the project location
                // from PowerShell and running
                //     dotnet user-secrets set "ServiceBus:ConnectionString" "<connection-string>"
                ConnectionString = configuration.GetValue<string>("ServiceBus:ConnectionString"),
                Topic = sbTopic
            };

            return services
                .AddHttpContextAccessor()
                .AddSingleton(topicConfiguration)
                .AddSingleton<IServiceBusTopicSender, ServiceBusTopicSender>()
                .AddSingleton<IETagProvider, ETagProvider>()
                .AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        }
    }
}