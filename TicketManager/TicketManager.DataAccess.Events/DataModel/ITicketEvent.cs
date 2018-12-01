namespace TicketManager.DataAccess.Events.DataModel
{
    public interface ITicketEvent
    {
        long TicketCreatedEventId { get; }

        TicketCreatedEvent TicketCreatedEvent { get; }
    }
}