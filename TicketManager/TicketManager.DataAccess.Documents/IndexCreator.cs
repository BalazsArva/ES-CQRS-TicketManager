using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace TicketManager.DataAccess.Documents
{
    public static class IndexCreator
    {
        public static void CreateIndexes(IDocumentStore documentStore)
        {
            IndexCreation.CreateIndexes(typeof(IndexCreator).Assembly, documentStore);
        }
    }
}