using MediatR;
using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketTypeCommand : IRequest
    {
        public int TicketId { get; set; }

        public string User { get; set; }

        public TicketType TicketType { get; set; }
    }
}