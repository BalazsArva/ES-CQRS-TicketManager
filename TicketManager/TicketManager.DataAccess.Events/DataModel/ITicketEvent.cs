namespace TicketManager.DataAccess.Events.DataModel
{
    public interface ITicketEvent
    {
        int TicketCreatedEventId { get; }

        TicketCreatedEvent TicketCreatedEvent { get; }
    }
}