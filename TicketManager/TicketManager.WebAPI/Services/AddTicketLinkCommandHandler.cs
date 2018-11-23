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
    public class AddTicketLinkCommandHandler : IRequestHandler<AddTicketLinkCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly AddTicketLinkCommandValidator addTicketLinkCommandValidator;

        public AddTicketLinkCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, AddTicketLinkCommandValidator addTicketLinkCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.addTicketLinkCommandValidator = addTicketLinkCommandValidator ?? throw new ArgumentNullException(nameof(addTicketLinkCommandValidator));
        }

        public async Task<Unit> Handle(AddTicketLinkCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await addTicketLinkCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            int ticketLinkChangedEventId;
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketLinkChangedEvent = new TicketLinkChangedEvent
                {
                    CausedBy = request.User,
                    LinkType = request.LinkType,
                    SourceTicketCreatedEventId = request.SourceTicketId,
                    TargetTicketCreatedEventId = request.TargetTicketId,
                    UtcDateRecorded = DateTime.UtcNow,
                    ConnectionIsActive = true
                };

                // TODO: Consider whether: there should be validation that the link is not yet established, OR simply ignore and get the distinct links on the query level.
                context.TicketLinkChangedEvents.Add(ticketLinkChangedEvent);

                await context.SaveChangesAsync();

                ticketLinkChangedEventId = ticketLinkChangedEvent.Id;
            }

            await mediator.Publish(new TicketLinkAddedNotification(ticketLinkChangedEventId));

            return Unit.Value;
        }
    }
}