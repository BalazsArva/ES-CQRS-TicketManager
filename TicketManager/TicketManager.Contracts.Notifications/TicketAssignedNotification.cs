﻿namespace TicketManager.Contracts.Notifications
{
    public class TicketAssignedNotification
    {
        public TicketAssignedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }

    public class TicketPriorityChangedNotification
    {
        public TicketPriorityChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }

    public class TicketStoryPointsChangedNotification
    {
        public TicketStoryPointsChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }

    public class TicketTagsChangedNotification
    {
        public TicketTagsChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }

    public class TicketUserInvolvementCancelledNotification
    {
        public TicketUserInvolvementCancelledNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}