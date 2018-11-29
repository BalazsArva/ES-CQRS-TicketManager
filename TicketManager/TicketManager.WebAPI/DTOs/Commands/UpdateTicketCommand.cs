﻿using MediatR;
using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class UpdateTicketCommand : IRequest
    {
        public class TicketLink
        {
            public int TargetTicketId { get; set; }

            public LinkType LinkType { get; set; }
        }

        public int TicketId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string User { get; set; }

        public string AssignedTo { get; set; }

        public Priority Priority { get; set; }

        public TicketType TicketType { get; set; }

        public TicketStatus TicketStatus { get; set; }

        public string[] Tags { get; set; }

        public TicketLink[] Links { get; set; }
    }
}