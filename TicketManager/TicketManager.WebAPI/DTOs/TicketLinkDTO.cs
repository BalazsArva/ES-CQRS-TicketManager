using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs
{
    public class TicketLinkDTO
    {
        public long TargetTicketId { get; set; }

        public TicketLinkTypes LinkType { get; set; }
    }
}