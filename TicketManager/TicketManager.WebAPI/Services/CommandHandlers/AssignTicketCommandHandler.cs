﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.Messaging.MessageClients.Abstractions;
using TicketManager.Messaging.Requests;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand>
    {
        private readonly ICorrelationIdProvider correlationIdProvider;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<AssignTicketCommand> validator;
        private readonly IMessagePublisher messagePublisher;

        public AssignTicketCommandHandler(ICorrelationIdProvider correlationIdProvider, IMessagePublisher messagePublisher, IEventsContextFactory eventsContextFactory, IValidator<AssignTicketCommand> validator)
        {
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();
            var ticketId = request.TicketId;

            var recordedEvent = new TicketAssignedEvent
            {
                CorrelationId = correlationId,
                AssignedTo = request.AssignTo,
                CausedBy = request.RaisedByUser,
                TicketCreatedEventId = ticketId
            };

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketAssignedEvents.Add(recordedEvent);

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await RaiseNotificationAsync(recordedEvent, correlationId, cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }

        private async Task RaiseNotificationAsync(TicketAssignedEvent newAssignmentEvent, string correlationId, CancellationToken cancellationToken)
        {
            using var context = eventsContextFactory.CreateContext();

            var ticketId = newAssignmentEvent.TicketCreatedEventId;
            var newAssignmentEventId = newAssignmentEvent.Id;

            var previousEvent = await context.TicketAssignedEvents
                .OfTicket(ticketId)
                .Before(newAssignmentEventId)
                .LatestAsync(cancellationToken)
                .ConfigureAwait(false);

            var notification = new TicketAssignmentChangedNotification(
                ticketId,
                newAssignmentEventId,
                newAssignmentEvent.AssignedTo,
                previousEvent?.AssignedTo,
                newAssignmentEvent.CausedBy,
                newAssignmentEvent.UtcDateRecorded);

            var message = new PublishMessageRequest<TicketAssignmentChangedNotification>(notification, correlationId);

            await messagePublisher.PublishMessageAsync(message);
        }
    }
}