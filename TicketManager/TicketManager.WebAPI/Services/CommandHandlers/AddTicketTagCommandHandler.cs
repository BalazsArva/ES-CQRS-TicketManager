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

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class AddTicketTagCommandHandler : IRequestHandler<AddTicketTagCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly AddTicketTagCommandValidator addTicketTagCommandValidator;

        public AddTicketTagCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, AddTicketTagCommandValidator addTicketTagCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.addTicketTagCommandValidator = addTicketTagCommandValidator ?? throw new ArgumentNullException(nameof(addTicketTagCommandValidator));
        }

        public async Task<Unit> Handle(AddTicketTagCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await addTicketTagCommandValidator.ValidateAsync(request, cancellationToken);
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
                    TagAdded = true,
                    TicketCreatedEventId = request.TicketId,
                    UtcDateRecorded = DateTime.UtcNow
                };

                // TODO: Consider whether: there should be validation that the tag is not yet assigned to the ticket, OR simply ignore and get the distinct tags on the query level.
                context.TicketTagChangedEvents.Add(ticketTagChangedEvent);

                await context.SaveChangesAsync();

                ticketTagChangedEventId = ticketTagChangedEvent.Id;
            }

            await mediator.Publish(new TicketTagAddedNotification(ticketTagChangedEventId));

            return Unit.Value;
        }
    }
}