namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketExtendedDetailsViewModel : TicketBasicDetailsViewModel
    {
        public string Description { get; set; }

        public TicketLinksViewModel Links { get; set; }
    }
}