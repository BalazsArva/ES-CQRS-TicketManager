namespace TicketManager.DataAccess.Documents.Utilities
{
    public interface IETagProvider
    {
        string CreateCombinedETagFromDocumentETags(params string[] eTags);
    }
}