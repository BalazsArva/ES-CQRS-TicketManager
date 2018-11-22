using Raven.Client.Documents.Session;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class IAsyncDocumentSessionExtensions
    {
        public static string GeneratePrefixedDocumentId<TDocument>(this IAsyncDocumentSession documentSession, TDocument entity, string customIdValue)
        {
            var separator = documentSession.Advanced.DocumentStore.Conventions.IdentityPartsSeparator;
            var collectionName = documentSession.Advanced.DocumentStore.Conventions.GetCollectionName(entity);

            return string.Concat(collectionName, separator, customIdValue);
        }
    }
}