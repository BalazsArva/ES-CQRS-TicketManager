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

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Title))
                {
                    result.TitleChanges = await GetTicketTitleChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Description))
                {
                    result.DescriptionChanges = await GetTicketDescriptionChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Type))
                {
                    result.TypeChanges = await GetTicketTypeChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Status))
                {
                    result.StatusChanges = await GetTicketStatusChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Priority))
                {
                    result.PriorityChanges = await GetTicketPriorityChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Assignment))
                {
                    result.AssignmentChanges = await GetTicketAssignmentChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Tags))
                {
                    result.TagChanges = await GetTicketTagChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                if (requestAll || requestedHistoryTypes.Contains(TicketHistoryTypes.Links))
                {
                    result.LinkChanges = await GetTicketLinkChangesAsync(context, ticketId, cancellationToken).ConfigureAwait(false);
                }

                // TODO: Add support for eTags
                return new QueryResult<GetTicketHistoryQueryResponse>(result);
            }
        }

        private async Task<IEnumerable<Change<string>>> GetTicketTitleChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }

        private async Task<IEnumerable<Change<string>>> GetTicketDescriptionChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }

        private async Task<IEnumerable<EnumChange<TicketStatuses>>> GetTicketStatusChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }

        private async Task<IEnumerable<EnumChange<TicketTypes>>> GetTicketTypeChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }

        private async Task<IEnumerable<EnumChange<TicketPriorities>>> GetTicketPriorityChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }

        private async Task<IEnumerable<Change<string>>> GetTicketAssignmentChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }

        private async Task<IEnumerable<Change<TicketTagChange>>> GetTicketTagChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }

        private async Task<IEnumerable<Change<TicketLinkChange>>> GetTicketLinkChangesAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var events = await context
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

            return events;
        }
    }
}