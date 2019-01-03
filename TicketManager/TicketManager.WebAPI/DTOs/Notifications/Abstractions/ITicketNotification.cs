using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications.Abstractions
{
    public interface ITicketNotification : INotification
    {
        long TicketId { get; }
    }
}