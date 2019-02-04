namespace TicketManager.WebAPI.DTOs.Queries.Abstractions
{
    public class GetTicketMetadataQueryResult
    {
        public GetTicketMetadataQueryResult(string eTag)
        {
            ETag = eTag;
        }

        public string ETag { get; }
    }
}