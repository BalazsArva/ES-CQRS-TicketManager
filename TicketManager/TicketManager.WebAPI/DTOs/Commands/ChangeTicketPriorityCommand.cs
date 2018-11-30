using MediatR;
using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketPriorityCommand : IRequest
    {
        public int TicketId { get; set; }

        public string User { get; set; }

        public Priority Priority { get; set; }
    }
}