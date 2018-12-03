namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ITagOperationCommand
    {
        string[] Tags { get; }
    }
}