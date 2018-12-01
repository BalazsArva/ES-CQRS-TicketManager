using System;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class RemoveTicketTagsCommand : TicketCommandBase
    {
        public string[] Tags { get; set; } = Array.Empty<string>();
    }
}