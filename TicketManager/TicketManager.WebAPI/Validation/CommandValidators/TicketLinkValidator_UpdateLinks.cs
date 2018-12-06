using Raven.Client.Documents;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class TicketLinkValidator_UpdateLinks : TicketLinkDTOValidatorBase
    {
        public TicketLinkValidator_UpdateLinks(IDocumentStore documentStore)
            : base(documentStore)
        {
        }
    }
}