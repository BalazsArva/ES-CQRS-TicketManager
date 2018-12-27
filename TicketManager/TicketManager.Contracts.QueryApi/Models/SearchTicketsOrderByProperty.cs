namespace TicketManager.Contracts.QueryApi.Models
{
    public enum SearchTicketsOrderByProperty
    {
        Id,

        UtcDateCreated,

        CreatedBy,

        UtcDateLastModified,

        LastModifiedBy,

        Title,

        AssignedTo,

        Status,

        Priority,

        Type
    }
}