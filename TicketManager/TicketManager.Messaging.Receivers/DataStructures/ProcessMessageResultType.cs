namespace TicketManager.Messaging.Receivers.DataStructures
{
    public enum ProcessMessageResultType
    {
        Success,

        PermanentError,

        TransientError
    }
}