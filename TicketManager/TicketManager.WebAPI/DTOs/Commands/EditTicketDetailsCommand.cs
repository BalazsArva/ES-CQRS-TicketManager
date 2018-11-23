using MediatR;
using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketDetailsCommand : IRequest
    {
        public int TicketId { get; set; }

        public string Editor { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Priority Priority { get; set; }

        public TicketType TicketType { get; set; }
    }
}