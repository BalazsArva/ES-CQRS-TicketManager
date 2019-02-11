namespace TicketManager.Messaging.Receivers.DataStructures
{
    public class ProcessMessageResult
    {
        protected ProcessMessageResult(ProcessMessageResultType resultType, string reason)
        {
            ResultType = resultType;
            Reason = reason;
        }

        public static ProcessMessageResult Success() => new ProcessMessageResult(ProcessMessageResultType.Success, null);

        public static ProcessMessageResult PermanentError(string reason) => new ProcessMessageResult(ProcessMessageResultType.PermanentError, reason);

        public static ProcessMessageResult TransientError(string reason) => new ProcessMessageResult(ProcessMessageResultType.TransientError, reason);

        public ProcessMessageResultType ResultType { get; }

        public string Reason { get; }
    }
}