namespace TicketManager.WebAPI.DTOs.Queries.Abstractions
{
    public class GetTicketMetadataQueryResult
    {
        public GetTicketMetadataQueryResult(Existences resultType)
            : this(resultType, null)
        {
        }

        public GetTicketMetadataQueryResult(Existences resultType, string eTag)
        {
            ResultType = resultType;
            ETag = eTag;
        }

        public static GetTicketMetadataQueryResult NotFound { get; } = new GetTicketMetadataQueryResult(Existences.NotFound, null);

        public Existences ResultType { get; }

        public string ETag { get; }
    }
}