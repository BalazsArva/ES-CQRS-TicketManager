using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.Contracts.Common;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.Messaging.MessageClients.Abstractions;
using TicketManager.Messaging.Requests;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class AddTicketLinksCommandHandler : IRequestHandler<AddTicketLinksCommand>
    {
        private readonly ICorrelationIdProvider correlationIdProvider;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<AddTicketLinksCommand> validator;
        private readonly IMessagePublisher messagePublisher;

        public AddTicketLinksCommandHandler(
            ICorrelationIdProvider correlationIdProvider,
            IMessagePublisher messagePublisher,
            IEventsContextFactory eventsContextFactory,
            IValidator<AddTicketLinksCommand> validator)
        {
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(AddTicketLinksCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();
            var ticketId = request.TicketId;
            var causedBy = request.RaisedByUser;

            var statusUpdated = false;
            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var ticketLink in request.Links)
                {
                    context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                    {
                        CorrelationId = correlationId,
                        CausedBy = causedBy,
                        LinkType = ticketLink.LinkType,
                        SourceTicketCreatedEventId = ticketId,
                        TargetTicketCreatedEventId = ticketLink.TargetTicketId,
                        ConnectionIsActive = true,
                    });

                    if (!statusUpdated && ticketLink.LinkType == TicketLinkTypes.BlockedBy)
                    {
                        statusUpdated = true;

                        context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                        {
                            CorrelationId = correlationId,
                            CausedBy = causedBy,
                            TicketCreatedEventId = ticketId,
                            TicketStatus = TicketStatuses.Blocked,
                            Reason = $"Automatically set to blocked because of adding a link with 'Blocked by' type to ticket #{ticketLink.TargetTicketId}",
                        });
                    }
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var linksChangedNotification = new PublishMessageRequest<TicketLinksChangedNotification>(new TicketLinksChangedNotification(ticketId), correlationId);

            await messagePublisher.PublishMessageAsync(linksChangedNotification);

            if (statusUpdated)
            {
                var message = new PublishMessageRequest<TicketStatusChangedNotification>(new TicketStatusChangedNotification(ticketId), correlationId);

                await messagePublisher.PublishMessageAsync(message);
            }

            return Unit.Value;
        }
    }
}