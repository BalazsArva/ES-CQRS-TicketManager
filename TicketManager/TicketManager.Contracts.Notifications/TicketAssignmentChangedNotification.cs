using System;

namespace TicketManager.Contracts.Notifications
{
    public class TicketAssignmentChangedNotification
    {
        public TicketAssignmentChangedNotification(long ticketId, long ticketAssignedEventId, string assignedTo, string previouslyAssignedTo, string causedBy, DateTime utcDateTimeChanged)
        {
            TicketId = ticketId;
            TicketAssignedEventId = ticketAssignedEventId;
            AssignedTo = assignedTo;
            PreviouslyAssignedTo = previouslyAssignedTo;
            CausedBy = causedBy;
            UtcDateTimeChanged = utcDateTimeChanged;
        }

        public long TicketId { get; }

        public long TicketAssignedEventId { get; }

        public string AssignedTo { get; }

        public string PreviouslyAssignedTo { get; }

        public string CausedBy { get; }

        public DateTime UtcDateTimeChanged { get; }
    }
}