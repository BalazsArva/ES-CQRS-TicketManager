using System.Collections.Generic;
using TicketManager.Contracts.Common;
using TicketManager.Contracts.QueryApi.Models.Abstractions;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketHistoryViewModel
    {
        public IEnumerable<ChangeViewModel<string>> TitleChanges { get; set; }

        public IEnumerable<ChangeViewModel<string>> DescriptionChanges { get; set; }

        public IEnumerable<ChangeViewModel<string>> AssignmentChanges { get; set; }

        public IEnumerable<ChangeViewModel<TicketTagChangeViewModel>> TagChanges { get; set; }

        public IEnumerable<ChangeViewModel<TicketLinkChangeViewModel>> LinkChanges { get; set; }

        public IEnumerable<EnumChangeViewModel<TicketStatuses>> StatusChanges { get; set; }

        public IEnumerable<EnumChangeViewModel<TicketTypes>> TypeChanges { get; set; }

        public IEnumerable<EnumChangeViewModel<TicketPriorities>> PriorityChanges { get; set; }
    }
}