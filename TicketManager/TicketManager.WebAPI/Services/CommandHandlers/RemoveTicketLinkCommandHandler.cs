using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Validation.CommandValidators;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class RemoveTicketLinkCommandHandler : IRequestHandler<RemoveTicketLinkCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly RemoveTicketLinkCommandValidator removeTicketLinkCommandValidator;

        public RemoveTicketLinkCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, RemoveTicketLinkCommandValidator removeTicketLinkCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.removeTicketLinkCommandValidator = removeTicketLinkCommandValidator ?? throw new ArgumentNullException(nameof(removeTicketLinkCommandValidator));
        }

        public async Task<Unit> Handle(RemoveTicketLinkCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await removeTicketLinkCommandValidator.ValidateAsync(request, cancellationToken);
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
                    ConnectionIsActive = false
                };

                // TODO: Consider whether: there should be validation that the link is already established, OR simply ignore as the query won't return it anyway.
                context.TicketLinkChangedEvents.Add(ticketLinkChangedEvent);

                await context.SaveChangesAsync();

                ticketLinkChangedEventId = ticketLinkChangedEvent.Id;
            }

            await mediator.Publish(new TicketLinkRemovedNotification(ticketLinkChangedEventId));

            return Unit.Value;
        }
    }
}