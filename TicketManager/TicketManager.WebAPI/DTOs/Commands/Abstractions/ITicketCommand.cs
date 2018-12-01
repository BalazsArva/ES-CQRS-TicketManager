namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ITicketCommand
    {
        long TicketId { get; }

        string RaisedByUser { get; }
    }
}