using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TicketManager.Contracts.Notifications;
using TicketManager.Receivers;
using TicketManager.Receivers.Configuration;
using TicketManager.Receivers.DataStructures;

namespace TicketManager.Notifications.TicketAssignmentChanged
{
    public class TicketAssignmentChangedNotificationReceiver : SubscriptionReceiverHostBase<TicketAssignmentChangedNotification>
    {
        public TicketAssignmentChangedNotificationReceiver(ServiceBusSubscriptionConfiguration subscriptionConfiguration)
            : base(subscriptionConfiguration)
        {
        }

        public override Task<ProcessMessageResult> HandleMessageAsync(TicketAssignmentChangedNotification notification, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            if (notification.AssignedTo != null)
            {
                var assignedToNotification = new
                {
                    SourceSystem = "Tickets",
                    UtcDateTimeCreated = notification.UtcDateTimeChanged,
                    Type = "TicketAssignedToUser",
                    User = notification.AssignedTo,
                    Title = $"{notification.CausedBy} has assigned a ticket to you"
                };
            }

            if (notification.PreviouslyAssignedTo != null)
            {
                var deassignmentNotification = new
                {
                    SourceSystem = "Tickets",
                    UtcDateTimeCreated = notification.UtcDateTimeChanged,
                    Type = "TicketDeassignedFromUser",
                    User = notification.PreviouslyAssignedTo,
                    Title = $"{notification.CausedBy} has deassigned a ticket from you"
                };
            }

            return Task.FromResult(ProcessMessageResult.Success());
        }
    }
}