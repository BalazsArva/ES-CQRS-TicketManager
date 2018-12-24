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
            var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var ticketLink in request.Links)
                {
                    context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        LinkType = ticketLink.LinkType,
                        SourceTicketCreatedEventId = request.TicketId,
                        TargetTicketCreatedEventId = ticketLink.TargetTicketId,
                        ConnectionIsActive = false
                    });
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketLinksRemovedNotification(request.TicketId), cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}