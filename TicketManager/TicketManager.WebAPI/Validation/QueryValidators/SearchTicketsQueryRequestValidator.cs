using FluentValidation;
using TicketManager.Contracts.Common;
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

            RuleFor(r => r.Status)
                .Must(BeValidCaseInsensitiveEnumString<TicketStatuses>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketStatuses>("status"))
                .When(r => !string.IsNullOrEmpty(r.Status));

            RuleFor(r => r.TicketType)
                .Must(BeValidCaseInsensitiveEnumString<TicketTypes>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketTypes>("ticket type"))
                .When(r => !string.IsNullOrEmpty(r.TicketType));

            RuleFor(r => r.Priority)
                .Must(BeValidCaseInsensitiveEnumString<TicketPriorities>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketPriorities>("priority"))
                .When(r => !string.IsNullOrEmpty(r.Priority));

            RuleFor(r => r.OrderBy)
                .Must(BeValidCaseInsensitiveEnumString<SearchTicketsOrderByProperty>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<SearchTicketsOrderByProperty>("sorting"));

            RuleFor(r => r.OrderDirection)
                .Must(BeValidCaseInsensitiveEnumString<OrderDirection>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<OrderDirection>("sorting direction"));
        }
    }
}