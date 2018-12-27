namespace TicketManager.WebAPI.DTOs.Queries.Abstractions
{

    public class TicketExistsQueryResult
    {
        public TicketExistsQueryResult(TicketExistsQueryResultType resultType)
            : this(resultType, null)
        {
        }

        public TicketExistsQueryResult(TicketExistsQueryResultType resultType, string eTag)
        {
            ResultType = resultType;
            ETag = eTag;
        }

        public static TicketExistsQueryResult NotFound { get; } = new TicketExistsQueryResult(TicketExistsQueryResultType.NotFound, null);

        public TicketExistsQueryResultType ResultType { get; }

        public string ETag { get; }
    }
}