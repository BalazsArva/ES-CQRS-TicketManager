using System.Linq;
using FluentValidation;
using TicketManager.WebAPI.DTOs.Queries;

namespace TicketManager.WebAPI.Validation.QueryValidators
{
    public class GetTicketHistoryQueryRequestValidator : ValidatorBase<GetTicketHistoryQueryRequest>
    {
        public GetTicketHistoryQueryRequestValidator()
        {
            RuleFor(r => r.TicketHistoryTypes)
                .Must(ContainOnlyValidSegments)
                .When(r => !string.IsNullOrEmpty(r.TicketHistoryTypes));
        }

        private bool ContainOnlyValidSegments(string value)
        {
            var parts = TicketHistoryTypes.GetRequestedHistoryTypes(value);

            return parts.All(part => TicketHistoryTypes.ValidValues.Contains(part));
        }
    }
}