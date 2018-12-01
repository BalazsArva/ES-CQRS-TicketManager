namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ITicketCommand
    {
        int TicketId { get; set; }

        string RaisedByUser { get; set; }
    }
}