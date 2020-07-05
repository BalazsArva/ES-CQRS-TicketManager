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
    public class ChangeTicketPriorityCommandHandler : IRequestHandler<ChangeTicketPriorityCommand>
    {
        private readonly IMessagePublisher messagePublisher;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<ChangeTicketPriorityCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;

        public ChangeTicketPriorityCommandHandler(ICorrelationIdProvider correlationIdProvider, IMessagePublisher messagePublisher, IEventsContextFactory eventsContextFactory, IValidator<ChangeTicketPriorityCommand> validator)
        {
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<Unit> Handle(ChangeTicketPriorityCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();
            var ticketId = request.TicketId;

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEventId = ticketId,
                    Priority = request.Priority
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var message = new PublishMessageRequest<TicketPriorityChangedNotification>(new TicketPriorityChangedNotification(ticketId), correlationId);

            await messagePublisher.PublishMessageAsync(message);

            return Unit.Value;
        }
    }
}