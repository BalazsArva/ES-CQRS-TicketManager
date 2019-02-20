using TicketManager.DataAccess.EntityFramework;

namespace TicketManager.DataAccess.Events
{
    public interface IEventsContextFactory : IDbContextFactory<EventsContext>
    {
    }
}