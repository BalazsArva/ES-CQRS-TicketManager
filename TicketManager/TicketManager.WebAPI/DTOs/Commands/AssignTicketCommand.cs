using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AssignTicketCommand : IRequest
    {
        public int TicketId { get; set; }

        public string Assigner { get; set; }

        public string AssignTo { get; set; }
    }
}