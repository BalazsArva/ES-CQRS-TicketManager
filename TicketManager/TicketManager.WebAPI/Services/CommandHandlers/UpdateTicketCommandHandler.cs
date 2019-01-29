using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Services.Providers;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;
        private readonly IValidator<UpdateTicketCommand> validator;
        private readonly ICorrelationIdProvider correlationIdProvider;

        public UpdateTicketCommandHandler(ICorrelationIdProvider correlationIdProvider, IMediator mediator, IEventsContextFactory eventsContextFactory, IDocumentStore documentStore, IValidator<UpdateTicketCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.correlationIdProvider = correlationIdProvider ?? throw new ArgumentNullException(nameof(correlationIdProvider));
        }

        public async Task<Unit> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var correlationId = correlationIdProvider.GetCorrelationId();

            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var updatedBy = request.RaisedByUser;
                var ticketId = request.TicketId;

                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                UpdateAssignmentIfChanged(context, ticketDocument, ticketId, request.AssignedTo, updatedBy, correlationId);
                UpdateTitleIfChanged(context, ticketDocument, ticketId, request.Title, updatedBy, correlationId);
                UpdateDescriptionIfChanged(context, ticketDocument, ticketId, request.Description, updatedBy, correlationId);
                UpdateStatusIfChanged(context, ticketDocument, ticketId, request.TicketStatus, updatedBy, correlationId);
                UpdateTypeIfChanged(context, ticketDocument, ticketId, request.TicketType, updatedBy, correlationId);
                UpdatePriorityIfChanged(context, ticketDocument, ticketId, request.Priority, updatedBy, correlationId);
                UpdateTagsIfChanged(context, ticketDocument, ticketId, request.Tags, updatedBy, correlationId);

                // This must come after UpdateStatusIfChanged because it overwrites the status to Blocked if a link with type 'BlockedBy' is found.
                UpdateLinksIfChanged(context, ticketDocument, ticketId, request.Links, updatedBy, correlationId);

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await mediator.Publish(new TicketUpdatedNotification(request.TicketId), cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }

        private void UpdateTitleIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string newTitle, string changedBy, string correlationId)
        {
            if (ticketDocument.TicketTitle?.Title != newTitle)
            {
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    Title = newTitle,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateDescriptionIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string newDescription, string changedBy, string correlationId)
        {
            if (ticketDocument.TicketDescription?.Description != newDescription)
            {
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    Description = newDescription,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateAssignmentIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string assignTo, string assignBy, string correlationId)
        {
            if (ticketDocument.Assignment?.AssignedTo != assignTo)
            {
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    CorrelationId = correlationId,
                    AssignedTo = assignTo,
                    CausedBy = assignBy,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateStatusIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, TicketStatuses newStatus, string changedBy, string correlationId)
        {
            if (ticketDocument.TicketStatus?.Status != newStatus)
            {
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    TicketCreatedEventId = ticketCreatedEventId,
                    TicketStatus = newStatus
                });
            }
        }

        private void UpdateTypeIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, TicketTypes newTicketType, string changedBy, string correlationId)
        {
            if (ticketDocument.TicketType?.Type != newTicketType)
            {
                context.TicketTypeChangedEvents.Add(new TicketTypeChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    TicketCreatedEventId = ticketCreatedEventId,
                    TicketType = newTicketType
                });
            }
        }

        private void UpdatePriorityIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, TicketPriorities newPriority, string changedBy, string correlationId)
        {
            if (ticketDocument.TicketPriority?.Priority != newPriority)
            {
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    TicketCreatedEventId = ticketCreatedEventId,
                    Priority = newPriority
                });
            }
        }

        private void UpdateTagsIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string[] newTags, string changedBy, string correlationId)
        {
            var currentlyAssignedTags = ticketDocument.Tags?.TagSet ?? Array.Empty<string>();

            var removedTags = currentlyAssignedTags.Except(newTags);
            var addedTags = newTags.Except(currentlyAssignedTags);

            foreach (var removedTag in removedTags)
            {
                context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    Tag = removedTag,
                    TagAdded = false,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }

            foreach (var addedTag in addedTags)
            {
                context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    Tag = addedTag,
                    TagAdded = true,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateLinksIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, TicketLinkDTO[] newLinks, string changedBy, string correlationId)
        {
            var currentLinks = ticketDocument.Links?.LinkSet ?? Array.Empty<TicketLink>();

            var removedLinks = currentLinks
                .Where(lnk => !newLinks.Any(newLnk => newLnk.LinkType == lnk.LinkType && documentStore.GeneratePrefixedDocumentId<Ticket>(newLnk.TargetTicketId) == lnk.TargetTicketId))
                .ToList();

            var addedLinks = newLinks
                .Select(newLnk => new TicketLink
                {
                    LinkType = newLnk.LinkType,
                    TargetTicketId = documentStore.GeneratePrefixedDocumentId<Ticket>(newLnk.TargetTicketId)
                })
                .Where(newLnk => !currentLinks.Any(currLnk => currLnk.LinkType == newLnk.LinkType && currLnk.TargetTicketId == newLnk.TargetTicketId))
                .ToList();

            foreach (var removedLink in removedLinks)
            {
                context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    ConnectionIsActive = false,
                    LinkType = removedLink.LinkType,
                    SourceTicketCreatedEventId = ticketCreatedEventId,
                    TargetTicketCreatedEventId = long.Parse(documentStore.TrimIdPrefix<Ticket>(removedLink.TargetTicketId))
                });
            }

            var setToBlocked = false;
            foreach (var addedLink in addedLinks)
            {
                var targetTicketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(addedLink.TargetTicketId));
                context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                {
                    CorrelationId = correlationId,
                    CausedBy = changedBy,
                    ConnectionIsActive = true,
                    LinkType = addedLink.LinkType,
                    SourceTicketCreatedEventId = ticketCreatedEventId,
                    TargetTicketCreatedEventId = targetTicketId
                });

                if (!setToBlocked && addedLink.LinkType == TicketLinkTypes.BlockedBy)
                {
                    setToBlocked = true;

                    context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                    {
                        CorrelationId = correlationId,
                        CausedBy = changedBy,
                        TicketCreatedEventId = ticketCreatedEventId,
                        TicketStatus = TicketStatuses.Blocked,
                        Reason = $"Automatically set to blocked because of adding a link with 'Blocked by' type to ticket #{targetTicketId}"
                    });
                }
            }
        }
    }
}