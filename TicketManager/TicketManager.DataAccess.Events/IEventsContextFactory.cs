namespace TicketManager.DataAccess.Events
{
    public interface IEventsContextFactory
    {
        EventsContext CreateContext();
    }
}