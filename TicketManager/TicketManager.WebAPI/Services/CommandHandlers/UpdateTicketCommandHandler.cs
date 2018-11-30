using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Validation.CommandValidators;

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;
        private readonly UpdateTicketCommandValidator updateTicketCommandValidator;

        public UpdateTicketCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IDocumentStore documentStore, UpdateTicketCommandValidator updateTicketCommandValidator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.updateTicketCommandValidator = updateTicketCommandValidator ?? throw new ArgumentNullException(nameof(updateTicketCommandValidator));
        }

        public async Task<Unit> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await updateTicketCommandValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var now = DateTime.UtcNow;
                var updatedBy = request.User;
                var ticketId = request.TicketId;

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                UpdateAssignmentIfChanged(context, ticketDocument, ticketId, request.AssignedTo, updatedBy, now);
                UpdateDetailsIfChanged(context, ticketDocument, ticketId, request.Title, request.Description, request.TicketType, request.Priority, updatedBy, now);
                UpdateStatusIfChanged(context, ticketDocument, ticketId, request.TicketStatus, updatedBy, now);
                UpdateTypeIfChanged(context, ticketDocument, ticketId, request.TicketType, updatedBy, now);
                UpdatePriorityIfChanged(context, ticketDocument, ticketId, request.Priority, updatedBy, now);
                UpdateTagsIfChanged(context, ticketDocument, ticketId, request.Tags, updatedBy, now);
                UpdateLinksIfChanged(context, session, ticketDocument, ticketId, request.Links, updatedBy, now);

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketUpdatedNotification(request.TicketId));

            return Unit.Value;
        }

        private void UpdateDetailsIfChanged(EventsContext context, Ticket ticketDocument, int ticketCreatedEventId, string newTitle, string newDescription, Domain.Common.TicketTypes newTicketType, Domain.Common.TicketPriorities newPriority, string changedBy, DateTime dateOfUpdate)
        {
            var details = ticketDocument.Details;

            if (details?.Title != newTitle || details?.Description != newDescription)
            {
                context.TicketDetailsChangedEvents.Add(new TicketDetailsChangedEvent
                {
                    CausedBy = changedBy,
                    Description = newDescription,
                    Title = newTitle,
                    UtcDateRecorded = dateOfUpdate,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateAssignmentIfChanged(EventsContext context, Ticket ticketDocument, int ticketCreatedEventId, string assignTo, string assignBy, DateTime dateOfUpdate)
        {
            if (ticketDocument.Assignment?.AssignedTo != assignTo)
            {
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    AssignedTo = assignTo,
                    UtcDateRecorded = dateOfUpdate,
                    CausedBy = assignBy,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateStatusIfChanged(EventsContext context, Ticket ticketDocument, int ticketCreatedEventId, Domain.Common.TicketStatuses newStatus, string changedBy, DateTime dateOfUpdate)
        {
            if (ticketDocument.TicketStatus?.Status != newStatus)
            {
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    CausedBy = changedBy,
                    TicketCreatedEventId = ticketCreatedEventId,
                    TicketStatus = newStatus,
                    UtcDateRecorded = dateOfUpdate
                });
            }
        }

        private void UpdateTypeIfChanged(EventsContext context, Ticket ticketDocument, int ticketCreatedEventId, Domain.Common.TicketTypes newTicketType, string changedBy, DateTime dateOfUpdate)
        {
            if (ticketDocument.TicketType?.Type != newTicketType)
            {
                context.TicketTypeChangedEvents.Add(new TicketTypeChangedEvent
                {
                    CausedBy = changedBy,
                    UtcDateRecorded = dateOfUpdate,
                    TicketCreatedEventId = ticketCreatedEventId,
                    TicketType = newTicketType
                });
            }
        }

        private void UpdatePriorityIfChanged(EventsContext context, Ticket ticketDocument, int ticketCreatedEventId, Domain.Common.TicketPriorities newPriority, string changedBy, DateTime dateOfUpdate)
        {
            if (ticketDocument.TicketPriority?.Priority != newPriority)
            {
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CausedBy = changedBy,
                    UtcDateRecorded = dateOfUpdate,
                    TicketCreatedEventId = ticketCreatedEventId,
                    Priority = newPriority
                });
            }
        }

        private void UpdateTagsIfChanged(EventsContext context, Ticket ticketDocument, int ticketCreatedEventId, string[] newTags, string changedBy, DateTime dateOfUpdate)
        {
            var currentlyAssignedTags = ticketDocument.Tags?.TagSet ?? Array.Empty<string>();

            var removedTags = currentlyAssignedTags.Except(newTags);
            var addedTags = newTags.Except(currentlyAssignedTags);

            foreach (var removedTag in removedTags)
            {
                context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                {
                    CausedBy = changedBy,
                    Tag = removedTag,
                    TagAdded = false,
                    TicketCreatedEventId = ticketCreatedEventId,
                    UtcDateRecorded = dateOfUpdate
                });
            }

            foreach (var addedTag in addedTags)
            {
                context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                {
                    CausedBy = changedBy,
                    Tag = addedTag,
                    TagAdded = true,
                    TicketCreatedEventId = ticketCreatedEventId,
                    UtcDateRecorded = dateOfUpdate
                });
            }
        }

        private void UpdateLinksIfChanged(EventsContext context, IAsyncDocumentSession session, Ticket ticketDocument, int ticketCreatedEventId, TicketLinkDTO[] newLinks, string changedBy, DateTime dateOfUpdate)
        {
            var currentLinks = ticketDocument.Links?.LinkSet ?? Array.Empty<TicketLink>();

            var removedLinks = currentLinks
                .Where(lnk => !newLinks.Any(newLnk => newLnk.LinkType == lnk.LinkType && session.GeneratePrefixedDocumentId<Ticket>(newLnk.TargetTicketId.ToString()) == lnk.TargetTicketId))
                .ToList();

            var addedLinks = newLinks
                .Select(newLnk => new TicketLink
                {
                    LinkType = newLnk.LinkType,
                    TargetTicketId = session.GeneratePrefixedDocumentId<Ticket>(newLnk.TargetTicketId.ToString())
                })
                .Where(newLnk => !currentLinks.Any(currLnk => currLnk.LinkType == newLnk.LinkType && currLnk.TargetTicketId == newLnk.TargetTicketId))
                .ToList();

            foreach (var removedLink in removedLinks)
            {
                context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                {
                    CausedBy = changedBy,
                    ConnectionIsActive = false,
                    LinkType = removedLink.LinkType,
                    SourceTicketCreatedEventId = ticketCreatedEventId,
                    TargetTicketCreatedEventId = int.Parse(session.TrimIdPrefix<Ticket>(removedLink.TargetTicketId)),
                    UtcDateRecorded = dateOfUpdate
                });
            }

            foreach (var addedLink in addedLinks)
            {
                context.TicketLinkChangedEvents.Add(new TicketLinkChangedEvent
                {
                    CausedBy = changedBy,
                    ConnectionIsActive = true,
                    LinkType = addedLink.LinkType,
                    SourceTicketCreatedEventId = ticketCreatedEventId,
                    TargetTicketCreatedEventId = int.Parse(session.TrimIdPrefix<Ticket>(addedLink.TargetTicketId)),
                    UtcDateRecorded = dateOfUpdate
                });
            }
        }
    }
}