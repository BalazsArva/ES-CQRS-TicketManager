using Microsoft.Extensions.DependencyInjection;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.BusinessServices.EventAggregators.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventAggregators(this IServiceCollection services)
        {
            services
                .AddTransient<IEventAggregator<Assignment>, TicketAssignedEventAggregator>()
                .AddTransient<IEventAggregator<TicketInvolvement>, TicketUserInvolvementEventAggregator>()
                .AddTransient<IEventAggregator<TicketDescription>, TicketDescriptionChangedEventAggregator>()
                .AddTransient<IEventAggregator<Links>, TicketLinksChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketPriority>, TicketPriorityChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketStatus>, TicketStatusChangedEventAggregator>()
                .AddTransient<IEventAggregator<Tags>, TicketTagsChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketTitle>, TicketTitleChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketType>, TicketTypeChangedEventAggregator>()
                .AddTransient<IEventAggregator<StoryPoints>, TicketStoryPointsChangedEventAggregator>()
                .AddTransient<IEventAggregator<TicketInvolvement>, TicketUserInvolvementEventAggregator>();

            return services;
        }
    }
}