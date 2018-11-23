using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AddTicketTagCommand : IRequest
    {
        public int TicketId { get; set; }

        public string User { get; set; }

        public string Tag { get; set; }
    }
}