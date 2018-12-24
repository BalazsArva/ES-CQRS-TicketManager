using System;

namespace TicketManager.WebAPI.DTOs.Queries.Abstractions
{
    public class Change<T>
    {
        public DateTime UtcDateChanged { get; set; }

        public string ChangedBy { get; set; }

        public virtual T ChangedTo { get; set; }
    }
}