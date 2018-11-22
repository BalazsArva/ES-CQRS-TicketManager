namespace TicketManager.WebAPI.DTOs.Queries
{
    public class QueryResult<TResult>
    {
        public TResult Result { get; set; }

        public QueryResultType ResultType { get; set; }

        public string ETag { get; set; }
    }
}