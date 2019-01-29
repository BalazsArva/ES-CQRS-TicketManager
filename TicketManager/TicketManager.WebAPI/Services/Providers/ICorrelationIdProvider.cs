namespace TicketManager.WebAPI.Services.Providers
{
    public interface ICorrelationIdProvider
    {
        string GetCorrelationId();
    }
}