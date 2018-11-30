using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs
{
    public class TicketLinkDTO
    {
        public int TargetTicketId { get; set; }

        public TicketLinkTypes LinkType { get; set; }
    }
}