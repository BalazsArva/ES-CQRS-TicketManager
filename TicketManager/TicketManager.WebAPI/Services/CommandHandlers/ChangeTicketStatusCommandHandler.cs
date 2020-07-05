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
    public class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<ChangeTicketStatusCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;
        private readonly IMessagePublisher messagePublisher;

        public ChangeTicketStatusCommandHandler(ICorrelationIdProvider correlationIdProvider, IMessagePublisher messagePublisher, IEventsContextFactory eventsContextFactory, IValidator<ChangeTicketStatusCommand> validator)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        }

        public async Task<Unit> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();
            var ticketId = request.TicketId;

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = ticketId,
                    TicketStatus = request.Status
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var message = new PublishMessageRequest<TicketStatusChangedNotification>(new TicketStatusChangedNotification(ticketId), correlationId);

            await messagePublisher.PublishMessageAsync(message);

            return Unit.Value;
        }
    }
}