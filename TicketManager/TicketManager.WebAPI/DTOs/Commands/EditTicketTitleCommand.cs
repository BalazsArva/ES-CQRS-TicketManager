using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketTitleCommand : IRequest
    {
        public int TicketId { get; set; }

        public string Editor { get; set; }

        public string Title { get; set; }
    }
}