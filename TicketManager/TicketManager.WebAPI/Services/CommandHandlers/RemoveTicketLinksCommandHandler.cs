using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.Messaging.MessageClients.Abstractions;
using TicketManager.Messaging.Requests;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class RemoveTicketLinksCommandHandler : IRequestHandler<RemoveTicketLinksCommand>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<RemoveTicketLinksCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;
        private readonly IMessagePublisher messagePublisher;

        public RemoveTicketLinksCommandHandler(ICorrelationIdProvider correlationIdProvider, IMessagePublisher messagePublisher, IEventsContextFactory eventsContextFactory, IValidator<RemoveTicketLinksCommand> validator)
        {
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<Unit> Handle(RemoveTicketLinksCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();
            var ticketId = request.TicketId;

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var ticketLink in request.Links)
                {
                    context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                    {
                        CorrelationId = correlationId,
                        CausedBy = request.RaisedByUser,
                        LinkType = ticketLink.LinkType,
                        SourceTicketCreatedEventId = ticketId,
                        TargetTicketCreatedEventId = ticketLink.TargetTicketId,
                        ConnectionIsActive = false
                    });
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var message = new PublishMessageRequest<TicketLinksChangedNotification>(new TicketLinksChangedNotification(ticketId), correlationId);

            await messagePublisher.PublishMessageAsync(message);

            return Unit.Value;
        }
    }
}