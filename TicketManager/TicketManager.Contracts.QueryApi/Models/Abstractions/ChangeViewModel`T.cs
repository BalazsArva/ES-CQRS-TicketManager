using System;

namespace TicketManager.Contracts.QueryApi.Models.Abstractions
{
    public class ChangeViewModel<T>
    {
        public DateTime UtcDateChanged { get; set; }

        public string ChangedBy { get; set; }

        public virtual T ChangedTo { get; set; }

        public string Reason { get; set; }
    }
}