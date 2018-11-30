namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketDetails : ChangeTrackedObjectBase
    {
        public string Title { get; set; }

        public string Description { get; set; }
    }
}