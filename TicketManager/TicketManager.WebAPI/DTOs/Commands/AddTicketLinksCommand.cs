﻿using System;
using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AddTicketLinksCommand : IRequest
    {
        public int SourceTicketId { get; set; }

        public string User { get; set; }

        public TicketLinkDTO[] Links { get; set; } = Array.Empty<TicketLinkDTO>();
    }
}