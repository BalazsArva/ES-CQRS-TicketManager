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
    public class AddTicketTagsCommandHandler : IRequestHandler<AddTicketTagsCommand>
    {
        private readonly ICorrelationIdProvider correlationIdProvider;
        private readonly IMessagePublisher messagePublisher;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<AddTicketTagsCommand> validator;

        public AddTicketTagsCommandHandler(ICorrelationIdProvider correlationIdProvider, IMessagePublisher messagePublisher, IEventsContextFactory eventsContextFactory, IValidator<AddTicketTagsCommand> validator)
        {
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(AddTicketTagsCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();
            var ticketId = request.TicketId;

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var tag in request.Tags)
                {
                    context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                    {
                        CorrelationId = correlationId,
                        CausedBy = request.RaisedByUser,
                        Tag = tag,
                        TagAdded = true,
                        TicketCreatedEventId = ticketId,
                    });
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var message = new PublishMessageRequest<TicketTagsChangedNotification>(new TicketTagsChangedNotification(ticketId), correlationId);

            await messagePublisher.PublishMessageAsync(message);

            return Unit.Value;
        }
    }
}