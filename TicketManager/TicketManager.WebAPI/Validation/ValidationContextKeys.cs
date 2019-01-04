namespace TicketManager.WebAPI.Validation
{
    public static class ValidationContextKeys
    {
        public const string TicketLinkOperationCommandContextDataKey = "TicketLinkOperationCommand";
        public const string CreateTicketCommandContextDataKey = "CreateTicketCommand";

        public const string FoundTicketIdsContextDataKey = "FoundTicketIds";
        public const string FoundCommentIdsContextDataKey = "FoundCommentIds";
        public const string FoundTicketTagsContextDataKey = "FoundTicketTags";
        public const string FoundTicketLinksContextDataKey = "FoundTicketLinks";
    }
}