using System;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketBasicDetails
    {
        // TODO: Consider making this immutable
        // TODO: Include status, type, etc. Will have to create a common contracts library and move the enums there as those will be required by the command API as well.
        public long Id { get; set; }

        public string Title { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAtUTC { get; set; }
    }
}