using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Notifications;
using TicketManager.DataAccess.Notifications.DataModel;
using TicketManager.Receivers;
using TicketManager.Receivers.Configuration;
using TicketManager.Receivers.DataStructures;

namespace TicketManager.Notifications.TicketAssignmentChanged
{
    public class TicketAssignmentChangedNotificationReceiver : SubscriptionReceiverHostBase<TicketAssignmentChangedNotification>
    {
        private const string SourceSystem = "Tickets";
        private const string AssignedToNotificationType = "TicketAssignedToUser";
        private const string DeassignedFromNotificationType = "TicketDeassignedFromUser";

        private readonly INotificationsContextFactory notificationsContextFactory;
        private readonly ITicketUrlProvider ticketUrlProvider;
        private readonly NotificationConfiguration configuration;

        public TicketAssignmentChangedNotificationReceiver(ServiceBusSubscriptionConfiguration subscriptionConfiguration, INotificationsContextFactory notificationsContextFactory, ITicketUrlProvider ticketUrlProvider, NotificationConfiguration configuration)
            : base(subscriptionConfiguration)
        {
            this.notificationsContextFactory = notificationsContextFactory ?? throw new ArgumentNullException(nameof(notificationsContextFactory));
            this.ticketUrlProvider = ticketUrlProvider ?? throw new ArgumentNullException(nameof(ticketUrlProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<ProcessMessageResult> HandleMessageAsync(TicketAssignmentChangedNotification notification, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            var causedBy = notification.CausedBy;
            var notificationTimestamp = notification.UtcDateTimeChanged;
            var ticketId = notification.TicketId;
            var assignedTo = notification.AssignedTo;
            var previouslyAssignedTo = notification.PreviouslyAssignedTo;

            using (var context = notificationsContextFactory.CreateContext())
            {
                if (assignedTo != null)
                {
                    context.Notifications.Add(new Notification
                    {
                        SourceSystem = SourceSystem,
                        UtcDateTimeCreated = notificationTimestamp,
                        Type = AssignedToNotificationType,
                        User = assignedTo,
                        Title = $"{causedBy} has assigned a ticket to you",
                        BrowserHref = ticketUrlProvider.GetBrowserUrl(ticketId),
                        ResourceHref = ticketUrlProvider.GetResourceUrl(ticketId),
                        IsRead = false,
                        IconUri = configuration.IconUrl
                    });
                }

                if (previouslyAssignedTo != null)
                {
                    context.Notifications.Add(new Notification
                    {
                        SourceSystem = SourceSystem,
                        UtcDateTimeCreated = notificationTimestamp,
                        Type = DeassignedFromNotificationType,
                        User = previouslyAssignedTo,
                        Title = $"{causedBy} has deassigned a ticket from you",
                        BrowserHref = ticketUrlProvider.GetBrowserUrl(ticketId),
                        ResourceHref = ticketUrlProvider.GetResourceUrl(ticketId),
                        IsRead = false,
                        IconUri = configuration.IconUrl
                    });
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return ProcessMessageResult.Success();
        }
    }

    public interface ITicketUrlProvider
    {
        string GetBrowserUrl(long ticketId);

        string GetResourceUrl(long ticketId);
    }

    public class TicketUrlProvider : ITicketUrlProvider
    {
        private readonly string browserUrlTemplate;
        private readonly string resourceUrlTemplate;

        public TicketUrlProvider(string browserUrlTemplate, string resourceUrlTemplate)
        {
            this.browserUrlTemplate = browserUrlTemplate ?? throw new ArgumentNullException(nameof(browserUrlTemplate));
            this.resourceUrlTemplate = resourceUrlTemplate ?? throw new ArgumentNullException(nameof(resourceUrlTemplate));
        }

        public string GetBrowserUrl(long ticketId)
        {
            return browserUrlTemplate.Replace("{ticketId}", ticketId.ToString());
        }

        public string GetResourceUrl(long ticketId)
        {
            return resourceUrlTemplate.Replace("{ticketId}", ticketId.ToString());
        }
    }

    public class NotificationConfiguration
    {
        public NotificationConfiguration(string iconUrl)
        {
            IconUrl = iconUrl;
        }

        public string IconUrl { get; }
    }
}