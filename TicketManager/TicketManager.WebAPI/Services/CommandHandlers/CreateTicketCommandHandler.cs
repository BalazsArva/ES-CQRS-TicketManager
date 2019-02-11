using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.Messaging.MessageClients;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, long>
    {
        private readonly ICorrelationIdProvider correlationIdProvider;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<CreateTicketCommand> validator;
        private readonly IServiceBusTopicSender serviceBusTopicSender;

        public CreateTicketCommandHandler(ICorrelationIdProvider correlationIdProvider, IServiceBusTopicSender serviceBusTopicSender, IEventsContextFactory eventsContextFactory, IValidator<CreateTicketCommand> validator)
        {
            this.serviceBusTopicSender = serviceBusTopicSender ?? throw new ArgumentNullException(nameof(serviceBusTopicSender));

            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<long> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            long ticketId;
            var correlationId = correlationIdProvider.GetCorrelationId();

            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketCreatedEvent = new TicketCreatedEvent { CausedBy = request.RaisedByUser, CorrelationId = correlationId, };

                context.TicketCreatedEvents.Add(ticketCreatedEvent);
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEvent = ticketCreatedEvent,
                    Title = request.Title
                });
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    Description = request.Description,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    Priority = request.Priority,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketTypeChangedEvents.Add(new TicketTypeChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    TicketType = request.Type,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    CorrelationId = correlationId,
                    TicketCreatedEvent = ticketCreatedEvent,
                    CausedBy = request.RaisedByUser,
                    TicketStatus = request.Status
                });
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    CorrelationId = correlationId,
                    AssignedTo = request.AssignTo,
                    CausedBy = request.RaisedByUser,
                    TicketCreatedEvent = ticketCreatedEvent
                });
                context.TicketStoryPointsChangedEvents.Add(new TicketStoryPointsChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = request.RaisedByUser,
                    StoryPoints = request.StoryPoints,
                    TicketCreatedEvent = ticketCreatedEvent
                });

                foreach (var tag in request.Tags)
                {
                    context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                    {
                        CorrelationId = correlationId,
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
                        CorrelationId = correlationId,
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
                            CorrelationId = correlationId,
                            CausedBy = request.RaisedByUser,
                            TicketCreatedEvent = ticketCreatedEvent,
                            TicketStatus = Contracts.Common.TicketStatuses.Blocked
                        });
                    }
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                ticketId = ticketCreatedEvent.Id;
            }

            await serviceBusTopicSender.SendAsync(new TicketCreatedNotification(ticketId), correlationId).ConfigureAwait(false);

            return ticketId;
        }
    }
}