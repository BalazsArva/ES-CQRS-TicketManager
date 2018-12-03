namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ILinkOperationCommand
    {
        TicketLinkDTO[] Links { get; }
    }
}