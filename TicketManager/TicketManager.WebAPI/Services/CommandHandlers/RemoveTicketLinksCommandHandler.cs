using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class RemoveTicketLinksCommandHandler : IRequestHandler<RemoveTicketLinksCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<RemoveTicketLinksCommand> validator;

        public RemoveTicketLinksCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<RemoveTicketLinksCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(RemoveTicketLinksCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                var now = DateTime.UtcNow;
                foreach (var ticketLink in request.Links)
                {
                    var ticketLinkChangedEvent = new TicketLinkChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        LinkType = ticketLink.LinkType,
                        SourceTicketCreatedEventId = request.TicketId,
                        TargetTicketCreatedEventId = ticketLink.TargetTicketId,
                        ConnectionIsActive = false
                    };

                    // TODO: Consider whether: there should be validation that the link is already established, OR simply ignore as the query won't return it anyway.

                    context.TicketLinkChangedEvents.Add(ticketLinkChangedEvent);
                }

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketLinksRemovedNotification(request.TicketId));

            return Unit.Value;
        }
    }
}