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

namespace TicketManager.WebAPI.Services.CommandHandlers
{
    public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand>
    {
        private readonly IMediator mediator;
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;
        private readonly IValidator<UpdateTicketCommand> validator;

        public UpdateTicketCommandHandler(IMediator mediator, IEventsContextFactory eventsContextFactory, IDocumentStore documentStore, IValidator<UpdateTicketCommand> validator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Unit> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var updatedBy = request.RaisedByUser;
                var ticketId = request.TicketId;

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                UpdateAssignmentIfChanged(context, ticketDocument, ticketId, request.AssignedTo, updatedBy);
                UpdateTitleIfChanged(context, ticketDocument, ticketId, request.Title, updatedBy);
                UpdateDescriptionIfChanged(context, ticketDocument, ticketId, request.Description, updatedBy);
                UpdateStatusIfChanged(context, ticketDocument, ticketId, request.TicketStatus, updatedBy);
                UpdateTypeIfChanged(context, ticketDocument, ticketId, request.TicketType, updatedBy);
                UpdatePriorityIfChanged(context, ticketDocument, ticketId, request.Priority, updatedBy);
                UpdateTagsIfChanged(context, ticketDocument, ticketId, request.Tags, updatedBy);
                UpdateLinksIfChanged(context, session, ticketDocument, ticketId, request.Links, updatedBy);

                await context.SaveChangesAsync();
            }

            await mediator.Publish(new TicketUpdatedNotification(request.TicketId));

            return Unit.Value;
        }

        private void UpdateTitleIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string newTitle, string changedBy)
        {
            if (ticketDocument.TicketTitle?.Title != newTitle)
            {
                context.TicketTitleChangedEvents.Add(new TicketTitleChangedEvent
                {
                    CausedBy = changedBy,
                    Title = newTitle,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateDescriptionIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string newDescription, string changedBy)
        {
            if (ticketDocument.TicketDescription?.Description != newDescription)
            {
                context.TicketDescriptionChangedEvents.Add(new TicketDescriptionChangedEvent
                {
                    CausedBy = changedBy,
                    Description = newDescription,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateAssignmentIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string assignTo, string assignBy)
        {
            if (ticketDocument.Assignment?.AssignedTo != assignTo)
            {
                context.TicketAssignedEvents.Add(new TicketAssignedEvent
                {
                    AssignedTo = assignTo,
                    CausedBy = assignBy,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateStatusIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, Domain.Common.TicketStatuses newStatus, string changedBy)
        {
            if (ticketDocument.TicketStatus?.Status != newStatus)
            {
                context.TicketStatusChangedEvents.Add(new TicketStatusChangedEvent
                {
                    CausedBy = changedBy,
                    TicketCreatedEventId = ticketCreatedEventId,
                    TicketStatus = newStatus
                });
            }
        }

        private void UpdateTypeIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, Domain.Common.TicketTypes newTicketType, string changedBy)
        {
            if (ticketDocument.TicketType?.Type != newTicketType)
            {
                context.TicketTypeChangedEvents.Add(new TicketTypeChangedEvent
                {
                    CausedBy = changedBy,
                    TicketCreatedEventId = ticketCreatedEventId,
                    TicketType = newTicketType
                });
            }
        }

        private void UpdatePriorityIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, Domain.Common.TicketPriorities newPriority, string changedBy)
        {
            if (ticketDocument.TicketPriority?.Priority != newPriority)
            {
                context.TicketPriorityChangedEvents.Add(new TicketPriorityChangedEvent
                {
                    CausedBy = changedBy,
                    TicketCreatedEventId = ticketCreatedEventId,
                    Priority = newPriority
                });
            }
        }

        private void UpdateTagsIfChanged(EventsContext context, Ticket ticketDocument, long ticketCreatedEventId, string[] newTags, string changedBy)
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
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }

            foreach (var addedTag in addedTags)
            {
                context.TicketTagChangedEvents.Add(new TicketTagChangedEvent
                {
                    CausedBy = changedBy,
                    Tag = addedTag,
                    TagAdded = true,
                    TicketCreatedEventId = ticketCreatedEventId
                });
            }
        }

        private void UpdateLinksIfChanged(EventsContext context, IAsyncDocumentSession session, Ticket ticketDocument, long ticketCreatedEventId, TicketLinkDTO[] newLinks, string changedBy)
        {
            var currentLinks = ticketDocument.Links?.LinkSet ?? Array.Empty<TicketLink>();

            var removedLinks = currentLinks
                .Where(lnk => !newLinks.Any(newLnk => newLnk.LinkType == lnk.LinkType && session.GeneratePrefixedDocumentId<Ticket>(newLnk.TargetTicketId) == lnk.TargetTicketId))
                .ToList();

            var addedLinks = newLinks
                .Select(newLnk => new TicketLink
                {
                    LinkType = newLnk.LinkType,
                    TargetTicketId = session.GeneratePrefixedDocumentId<Ticket>(newLnk.TargetTicketId)
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
                    TargetTicketCreatedEventId = int.Parse(session.TrimIdPrefix<Ticket>(removedLink.TargetTicketId))
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
                    TargetTicketCreatedEventId = int.Parse(session.TrimIdPrefix<Ticket>(addedLink.TargetTicketId))
                });
            }
        }
    }
}