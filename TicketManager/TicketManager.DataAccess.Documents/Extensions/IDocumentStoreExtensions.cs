using System;
using Raven.Client.Documents;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class IDocumentStoreExtensions
    {
        public static string GeneratePrefixedDocumentId<TDocument>(this IDocumentStore store, long customIdValue)
        {
            var separator = store.Conventions.IdentityPartsSeparator;
            var collectionName = store.Conventions.GetCollectionName(typeof(TDocument));

            return string.Concat(collectionName, separator, customIdValue.ToString());
        }

        public static string TrimIdPrefix<TDocument>(this IDocumentStore store, string documentId)
        {
            var separator = store.Conventions.IdentityPartsSeparator;
            var collectionName = store.Conventions.GetCollectionName(typeof(TDocument));

            var prefix = collectionName + separator;
            if (!documentId.StartsWith(prefix))
            {
                throw new ArgumentException($"The parameter '{nameof(documentId)}' must start with '{prefix}' to be able to remove the prefix.", nameof(documentId));
            }

            return documentId.Substring(prefix.Length);
        }
    }
}