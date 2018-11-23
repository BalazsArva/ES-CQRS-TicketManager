using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Validation;

namespace TicketManager.WebAPI.Services
{
    public class RemoveTicketTagCommandHandler : IRequestHandler<RemoveTicketTagCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly RemoveTicketTagCommandValidator removeTicketTagCommandValidator;

        public RemoveTicketTagCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, RemoveTicketTagCommandValidator removeTicketTagCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.removeTicketTagCommandValidator = removeTicketTagCommandValidator ?? throw new ArgumentNullException(nameof(removeTicketTagCommandValidator));
        }

        public async Task<Unit> Handle(RemoveTicketTagCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await removeTicketTagCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            int ticketTagChangedEventId;
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketTagChangedEvent = new TicketTagChangedEvent
                {
                    CausedBy = request.User,
                    Tag = request.Tag,
                    TagAdded = false,
                    TicketCreatedEventId = request.TicketId,
                    UtcDateRecorded = DateTime.UtcNow
                };

                // TODO: Consider whether: there should be validation that the tag is already assigned to the ticket, OR simply ignore as the query won't return it anyway.
                context.TicketTagChangedEvents.Add(ticketTagChangedEvent);

                await context.SaveChangesAsync();

                ticketTagChangedEventId = ticketTagChangedEvent.Id;
            }

            await mediator.Publish(new TicketTagRemovedNotification(ticketTagChangedEventId));

            return Unit.Value;
        }
    }
}