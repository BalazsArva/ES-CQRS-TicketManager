﻿namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ITicketCommand
    {
        int TicketId { get; }

        string RaisedByUser { get; }
    }
}