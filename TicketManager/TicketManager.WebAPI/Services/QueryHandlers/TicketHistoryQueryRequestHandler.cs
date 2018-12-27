using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;
using TicketManager.WebAPI.Extensions.Linq;

namespace TicketManager.WebAPI.Services.QueryHandlers
{
    public class TicketHistoryQueryRequestHandler : IRequestHandler<GetTicketHistoryQueryRequest, QueryResult<GetTicketHistoryQueryResponse>>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IValidator<GetTicketHistoryQueryRequest> validator;

        public TicketHistoryQueryRequestHandler(IEventsContextFactory eventsContextFactory, IValidator<GetTicketHistoryQueryRequest> validator)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<QueryResult<GetTicketHistoryQueryResponse>> Handle(GetTicketHistoryQueryRequest request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketId = request.TicketId;
                var requestedHistoryTypes = TicketHistoryTypes.GetRequestedHistoryTypes(request.TicketHistoryTypes);
                var requestAll = !requestedHistoryTypes.Any();

                var result = new GetTicketHistoryQueryResponse();

                await Task.WhenAll(new List<Task>
                {
                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Title))
                        ? PopulateTicketTitleChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask,

                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Description))
                        ? PopulateTicketDescriptionChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask,

                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Type))
                        ? PopulateTicketTypeChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask,

                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Status))
                        ? PopulateTicketStatusChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask,

                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Priority))
                        ? PopulateTicketPriorityChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask,

                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Assignment))
                        ? PopulateTicketAssignmentChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask,

                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Tags))
                        ? PopulateTicketTagChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask,

                    (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Links))
                        ? PopulateTicketLinkChangesAsync(result, context, ticketId, cancellationToken)
                        : Task.CompletedTask
                }).ConfigureAwait(false);

                // TODO: Add support for eTags. Also consider that even though a client may have a cached version, it might contain different history types from what is requested.
                return new QueryResult<GetTicketHistoryQueryResponse>(result);
            }
        }

        private async Task PopulateTicketTitleChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketTitleChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new Change<string>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = evt.Title,
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.TitleChanges = result;
        }

        private async Task PopulateTicketDescriptionChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketDescriptionChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new Change<string>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = evt.Description,
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.DescriptionChanges = result;
        }

        private async Task PopulateTicketStatusChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketStatusChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new EnumChange<TicketStatuses>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = evt.TicketStatus,
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.StatusChanges = result;
        }

        private async Task PopulateTicketTypeChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketTypeChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new EnumChange<TicketTypes>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = evt.TicketType,
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.TypeChanges = result;
        }

        private async Task PopulateTicketPriorityChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketPriorityChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new EnumChange<TicketPriorities>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = evt.Priority,
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.PriorityChanges = result;
        }

        private async Task PopulateTicketAssignmentChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketAssignedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new Change<string>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = evt.AssignedTo,
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.AssignmentChanges = result;
        }

        private async Task PopulateTicketTagChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketTagChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new Change<TicketTagChange>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = new TicketTagChange
                    {
                        Operation = evt.TagAdded ? TicketTagOperationTypes.Add : TicketTagOperationTypes.Remove,
                        Tag = evt.Tag
                    },
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.TagChanges = result;
        }

        private async Task PopulateTicketLinkChangesAsync(GetTicketHistoryQueryResponse response, EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var result = await context
                .TicketLinkChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .OrderBy(evt => evt.Id)
                .Select(evt => new Change<TicketLinkChange>
                {
                    ChangedBy = evt.CausedBy,
                    ChangedTo = new TicketLinkChange
                    {
                        Operation = evt.ConnectionIsActive ? TicketLinkOperationTypes.Add : TicketLinkOperationTypes.Remove,
                        LinkType = evt.LinkType,
                        SourceTicketId = evt.SourceTicketCreatedEventId,
                        TargetTicketId = evt.TargetTicketCreatedEventId
                    },
                    UtcDateChanged = evt.UtcDateRecorded
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            response.LinkChanges = result;
        }
    }
}