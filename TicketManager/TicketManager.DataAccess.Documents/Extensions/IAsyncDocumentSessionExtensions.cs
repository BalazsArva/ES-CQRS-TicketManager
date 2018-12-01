using System;
using Raven.Client.Documents.Session;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class IAsyncDocumentSessionExtensions
    {
        public static string GeneratePrefixedDocumentId<TDocument>(this IAsyncDocumentSession documentSession, long customIdValue)
        {
            var separator = documentSession.Advanced.DocumentStore.Conventions.IdentityPartsSeparator;
            var collectionName = documentSession.Advanced.DocumentStore.Conventions.GetCollectionName(typeof(TDocument));

            return string.Concat(collectionName, separator, customIdValue.ToString());
        }

        public static string TrimIdPrefix<TDocument>(this IAsyncDocumentSession documentSession, string documentId)
        {
            var separator = documentSession.Advanced.DocumentStore.Conventions.IdentityPartsSeparator;
            var collectionName = documentSession.Advanced.DocumentStore.Conventions.GetCollectionName(typeof(TDocument));

            var prefix = collectionName + separator;
            if (!documentId.StartsWith(prefix))
            {
                throw new ArgumentException($"The parameter '{nameof(documentId)}' must start with '{prefix}' to be able to remove the prefix.", nameof(documentId));
            }

            return documentId.Substring(prefix.Length);
        }
    }
}