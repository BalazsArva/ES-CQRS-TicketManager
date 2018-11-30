using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketDescriptionCommand : IRequest
    {
        public int TicketId { get; set; }

        public string Editor { get; set; }

        public string Description { get; set; }
    }
}