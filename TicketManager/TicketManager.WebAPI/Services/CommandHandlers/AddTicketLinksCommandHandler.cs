using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class AddTicketLinksCommandHandler : IRequestHandler<AddTicketLinksCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<AddTicketLinksCommand> validator;

        public AddTicketLinksCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<AddTicketLinksCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(AddTicketLinksCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var ticketId = request.TicketId;
            var causedBy = request.RaisedByUser;

            var statusUpdated = false;
            using (var context = eventsContextFactory.CreateContext())
            {
                foreach (var ticketLink in request.Links)
                {
                    context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                    {
                        CausedBy = causedBy,
                        LinkType = ticketLink.LinkType,
                        SourceTicketCreatedEventId = ticketId,
                        TargetTicketCreatedEventId = ticketLink.TargetTicketId,
                        ConnectionIsActive = true
                    });

                    if (!statusUpdated && ticketLink.LinkType == TicketLinkTypes.BlockedBy)
                    {
                        statusUpdated = true;

                        context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                        {
                            CausedBy = causedBy,
                            TicketCreatedEventId = ticketId,
                            TicketStatus = TicketStatuses.Blocked
                        });
                    }
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketLinksAddedNotification(ticketId), cancellationToken).ConfigureAwait(false);

            if (statusUpdated)
            {
                await mediator.Publish(new TicketStatusChangedNotification(ticketId), cancellationToken).ConfigureAwait(false);
            }

            return Unit.Value;
        }
    }
}