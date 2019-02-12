using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace TicketManager.Messaging.Setup
{
    public class ServiceBusConfigurer : IServiceBusConfigurer
    {
        private const string EventTypeRuleName = "EventType";

        public Task SetupSubscriptionAsync<TNotification>(ServiceBusSubscriptionSetup setupInfo, CancellationToken cancellationToken)
        {
            var eventType = typeof(TNotification).FullName;

            return SetupSubscriptionAsync(setupInfo.ConnectionString, setupInfo.Topic, setupInfo.Subscription, eventType, setupInfo.UseSessions, cancellationToken);
        }

        private async Task SetupSubscriptionAsync(string connectionString, string topic, string subscription, string eventType, bool useSessions, CancellationToken cancellationToken)
        {
            var managementClient = new ManagementClient(connectionString);

            var topicExists = await managementClient.TopicExistsAsync(topic, cancellationToken);
            if (!topicExists)
            {
                await managementClient.CreateTopicAsync(topic, cancellationToken);
            }

            var ruleDescription = new RuleDescription(EventTypeRuleName, new CorrelationFilter { Label = eventType });

            var subscriptionExists = await managementClient.SubscriptionExistsAsync(topic, subscription, cancellationToken);
            if (!subscriptionExists)
            {
                var subscriptionDescription = new SubscriptionDescription(topic, subscription)
                {
                    EnableDeadLetteringOnFilterEvaluationExceptions = false,
                    EnableDeadLetteringOnMessageExpiration = true,
                    RequiresSession = useSessions
                };

                await managementClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription, cancellationToken);

                return;
            }

            try
            {
                var subscriptionEventFilter = await managementClient.GetRuleAsync(topic, subscription, EventTypeRuleName, cancellationToken);

                await managementClient.UpdateRuleAsync(topic, subscription, ruleDescription);
            }
            catch (MessagingEntityNotFoundException)
            {
                await managementClient.CreateRuleAsync(topic, subscription, ruleDescription);
            }
        }
    }
}