namespace TicketManager.WebAPI.DTOs.Queries.Abstractions
{

    public class ExistenceCheckQueryResult
    {
        public ExistenceCheckQueryResult(ExistenceCheckQueryResultType resultType)
            : this(resultType, null)
        {
        }

        public ExistenceCheckQueryResult(ExistenceCheckQueryResultType resultType, string eTag)
        {
            ResultType = resultType;
            ETag = eTag;
        }

        public static ExistenceCheckQueryResult NotFound { get; } = new ExistenceCheckQueryResult(ExistenceCheckQueryResultType.NotFound, null);

        public ExistenceCheckQueryResultType ResultType { get; }

        public string ETag { get; }
    }
}