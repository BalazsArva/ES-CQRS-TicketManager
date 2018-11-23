using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class RemoveTicketTagCommand : IRequest
    {
        public int TicketId { get; set; }

        public string User { get; set; }

        public string Tag { get; set; }
    }
}