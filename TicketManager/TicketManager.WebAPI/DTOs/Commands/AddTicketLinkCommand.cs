using MediatR;
using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AddTicketLinkCommand : IRequest
    {
        public int SourceTicketId { get; set; }

        public int TargetTicketId { get; set; }

        public string User { get; set; }

        public LinkType LinkType { get; set; }
    }
}