using FluentValidation;
using TicketManager.Contracts.QueryApi;
using TicketManager.Contracts.QueryApi.Models;
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
                .Must(BeValidCaseInsensitiveEnumString<SearchTicketsOrderByProperty>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<SearchTicketsOrderByProperty>("sorting"));

            RuleFor(r => r.OrderDirection)
                .Must(BeValidCaseInsensitiveEnumString<OrderDirection>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<OrderDirection>("sorting direction"));
        }
    }
}