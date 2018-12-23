using FluentValidation;
using TicketManager.WebAPI.DTOs.Queries;

namespace TicketManager.WebAPI.Validation.QueryValidators
{
    public class SearchTicketsQueryRequestValidator : ValidatorBase<SearchTicketsQueryRequest>
    {
        private const int MinPage = 1;
        private const int MinPageSize = 1;

        public SearchTicketsQueryRequestValidator()
        {
            RuleFor(r => r.Page)
                .GreaterThanOrEqualTo(MinPage)
                .WithMessage(ValidationMessageProvider.MustBeAtLeast("page", MinPage));

            RuleFor(r => r.PageSize)
                .GreaterThanOrEqualTo(MinPageSize)
                .WithMessage(ValidationMessageProvider.MustBeAtLeast("page size", MinPageSize));

            RuleFor(r => r.OrderBy)
                .Must(BeValidEnumString<SearchTicketsQueryRequest.OrderByProperty>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<SearchTicketsQueryRequest.OrderByProperty>("sorting"));

            RuleFor(r => r.OrderDirection)
                .Must(BeValidEnumString<OrderDirection>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<OrderDirection>("sorting direction"));
        }
    }
}