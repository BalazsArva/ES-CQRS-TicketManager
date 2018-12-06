namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ILinkOperationCommand
    {
        long TicketId { get; }

        TicketLinkDTO[] Links { get; }
    }
}