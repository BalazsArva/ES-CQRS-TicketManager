using System.Collections.Generic;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class GetTicketHistoryQueryResponse
    {
        public IEnumerable<Change<string>> TitleChanges { get; set; }

        public IEnumerable<Change<string>> DescriptionChanges { get; set; }

        public IEnumerable<Change<string>> AssignmentChanges { get; set; }

        public IEnumerable<Change<TicketTagChange>> TagChanges { get; set; }

        public IEnumerable<Change<TicketLinkChange>> LinkChanges { get; set; }

        public IEnumerable<EnumChange<TicketStatuses>> StatusChanges { get; set; }

        public IEnumerable<EnumChange<TicketTypes>> TypeChanges { get; set; }

        public IEnumerable<EnumChange<TicketPriorities>> PriorityChanges { get; set; }
    }
}