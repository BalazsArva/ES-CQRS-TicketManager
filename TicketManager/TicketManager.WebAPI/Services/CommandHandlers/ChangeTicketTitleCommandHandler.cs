using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.Messaging.MessageClients;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class ChangeTicketTitleCommandHandler : IRequestHandler<ChangeTicketTitleCommand>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<ChangeTicketTitleCommand> validator;
        private readonly IServiceBusTopicSender serviceBusTopicSender;
        private readonly ICorrelationIdProvider correlationIdProvider;

        public ChangeTicketTitleCommandHandler(ICorrelationIdProvider correlationIdProvider, IServiceBusTopicSender serviceBusTopicSender, IEventsContextFactory eventsContextFactory, IValidator<ChangeTicketTitleCommand> validator)
        {
            this.serviceBusTopicSender = serviceBusTopicSender ?? throw new ArgumentNullException(nameof(serviceBusTopicSender));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<Unit> Handle(ChangeTicketTitleCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();
            var ticketId = request.TicketId;

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = ticketId,
                    Title = request.Title
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await serviceBusTopicSender.SendAsync(new TicketTitleChangedNotification(ticketId), correlationId).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}