using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.DataAccess.Documents.Utilities;
using TicketManager.Messaging.MessageClients;
using TicketManager.Messaging.MessageClients.Abstractions;
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

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services
                .AddHttpContextAccessor()
                .AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>()
                .AddSingleton<IETagProvider, ETagProvider>()
                .AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        }
    }
}