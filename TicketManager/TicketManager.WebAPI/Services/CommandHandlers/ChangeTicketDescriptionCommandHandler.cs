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
    public class ChangeTicketDescriptionCommandHandler : IRequestHandler<ChangeTicketDescriptionCommand>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<ChangeTicketDescriptionCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;
        private readonly IMessagePublisher messagePublisher;

        public ChangeTicketDescriptionCommandHandler(ICorrelationIdProvider correlationIdProvider, IMessagePublisher messagePublisher, IEventsContextFactory eventsContextFactory, IValidator<ChangeTicketDescriptionCommand> validator)
        {
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<Unit> Handle(ChangeTicketDescriptionCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();

            using (var context = eventsContextFactory.CreateContext())
            {
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    Description = request.Description,
                    TicketCreatedEventId = request.TicketId
                });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var message = new PublishMessageRequest<TicketDescriptionChangedNotification>(new TicketDescriptionChangedNotification(request.TicketId), correlationId);

            await messagePublisher.PublishMessageAsync(message);

            return Unit.Value;
        }
    }
}