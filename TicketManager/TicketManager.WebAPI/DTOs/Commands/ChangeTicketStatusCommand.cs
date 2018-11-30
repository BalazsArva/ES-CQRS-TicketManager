using MediatR;
using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketStatusCommand : IRequest
    {
        public int TicketId { get; set; }

        public string User { get; set; }

        public TicketStatuses NewStatus { get; set; }
    }
}