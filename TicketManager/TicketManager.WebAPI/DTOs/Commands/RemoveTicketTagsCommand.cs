﻿using System;
using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class RemoveTicketTagsCommand : IRequest
    {
        public int TicketId { get; set; }

        public string User { get; set; }

        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}