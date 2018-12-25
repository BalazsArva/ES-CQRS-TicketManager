namespace TicketManager.WebAPI.DTOs.Queries.Abstractions
{
    public class QueryResult<TResult>
    {
        public QueryResult(TResult result)
            : this(result, QueryResultType.Success, null)
        {
        }

        public QueryResult(TResult result, QueryResultType resultType, string eTag)
        {
            Result = result;
            ResultType = resultType;
            ETag = eTag;
        }

        public static QueryResult<TResult> NotFound { get; } = new QueryResult<TResult>(default, QueryResultType.NotFound, null);

        public static QueryResult<TResult> NotModified { get; } = new QueryResult<TResult>(default, QueryResultType.NotModified, null);

        public TResult Result { get; }

        public QueryResultType ResultType { get; }

        public string ETag { get; }
    }
}