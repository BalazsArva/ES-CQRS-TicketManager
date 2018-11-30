﻿using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    /// <summary>
    /// Represents a notification that is published when many updates to a ticket happened and the entire ticket should
    /// be synchronized.
    /// </summary>
    public class TicketUpdatedNotification : INotification
    {
        public TicketUpdatedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}