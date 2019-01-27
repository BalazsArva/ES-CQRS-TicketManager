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
    public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, long>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<CreateTicketCommand> validator;

        public CreateTicketCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IValidator<CreateTicketCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<long> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            long ticketId;
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketCreatedEvent = new TicketCreatedEvent { CausedBy = request.RaisedByUser };

                context.TicketCreatedEvents.Add(ticketCreatedEvent);
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEvent = ticketCreatedEvent,
                    Title = request.Title
                });
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    Description = request.Description,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    Priority = request.Priority,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketTypeChangedEvents.Add(new TicketTypeChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    TicketType = request.TicketType,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    TicketCreatedEvent = ticketCreatedEvent,
                    CausedBy = request.RaisedByUser,
                    TicketStatus = request.TicketStatus
                });
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    AssignedTo = request.AssignTo,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketStoryPointsChangedEvents.Add(new TicketStoryPointsChangedEvent
                {
                    CausedBy = request.RaisedByUser,
                    StoryPoints = request.StoryPoints,
                    TicketCreatedEvent = ticketCreatedEvent
                });

                foreach (var tag in request.Tags)
                {
                    context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        Tag = tag,
                        TagAdded = true,
                        TicketCreatedEvent = ticketCreatedEvent
                    });
                }

                foreach (var link in request.Links)
                {
                    context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                    {
                        CausedBy = request.RaisedByUser,
                        LinkType = link.LinkType,
                        ConnectionIsActive = true,
                        SourceTicketCreatedEvent = ticketCreatedEvent,
                        TargetTicketCreatedEventId = link.TargetTicketId
                    });

                    // Pay attention that this must always come after the addition of the status event for the user-set status as this one must overwrite that.
                    if (link.LinkType == Contracts.Common.TicketLinkTypes.BlockedBy)
                    {
                        context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                        {
                            CausedBy = request.RaisedByUser,
                            TicketCreatedEvent = ticketCreatedEvent,
                            TicketStatus = Contracts.Common.TicketStatuses.Blocked
                        });
                    }
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                ticketId = ticketCreatedEvent.Id;
            }

            await mediator.Publish(new TicketCreatedNotification(ticketId), cancellationToken).ConfigureAwait(false);

            return ticketId;
        }
    }
}