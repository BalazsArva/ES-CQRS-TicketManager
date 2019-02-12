using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace TicketManager.Messaging.Setup
{
    public class ServiceBusConfigurer : IServiceBusConfigurer
    {
        private const string EventTypeRuleName = "EventType";

        public async Task SetupSubscriptionAsync(string connectionString, string topic, string subscription, string eventType, CancellationToken cancellationToken)
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
                    EnableDeadLetteringOnMessageExpiration = true
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