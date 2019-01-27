using FluentValidation;
using TicketManager.Contracts.Common;
using TicketManager.Contracts.QueryApi;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.WebAPI.DTOs.Queries;

namespace TicketManager.WebAPI.Validation.QueryValidators
{
    public class SearchTicketsQueryRequestValidator : ValidatorBase<SearchTicketsQueryRequest>
    {
        // TODO: Move to ValidationConstants
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

            RuleFor(r => r.DateCreatedFrom)
                .Must(BeValidIsoDateString)
                .WithMessage(ValidationMessageProvider.MustBeValidIsoDateString("date created from"))
                .When(r => !string.IsNullOrEmpty(r.DateCreatedFrom));

            RuleFor(r => r.DateCreatedTo)
                .Must(BeValidIsoDateString)
                .WithMessage(ValidationMessageProvider.MustBeValidIsoDateString("date created to"))
                .When(r => !string.IsNullOrEmpty(r.DateCreatedTo));

            RuleFor(r => r.DateLastModifiedFrom)
                .Must(BeValidIsoDateString)
                .WithMessage(ValidationMessageProvider.MustBeValidIsoDateString("date last modified from"))
                .When(r => !string.IsNullOrEmpty(r.DateLastModifiedFrom));

            RuleFor(r => r.DateLastModifiedTo)
                .Must(BeValidIsoDateString)
                .WithMessage(ValidationMessageProvider.MustBeValidIsoDateString("date last modified to"))
                .When(r => !string.IsNullOrEmpty(r.DateLastModifiedTo));

            RuleFor(r => r.Status)
                .Must(BeValidCaseInsensitiveEnumString<TicketStatuses>)
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketStatuses>("status"))
                .When(r => !string.IsNullOrEmpty(r.Status));

            // TODO: Consider receiving the value as a string and check whether it can be converted to int. Without this, an invalid value would just be ignored as the parameter binding will silently fail.
            RuleFor(r => r.StoryPoints)
                .Transform(p => p.Value)
                .GreaterThanOrEqualTo(ValidationConstants.MinStoryPoints)
                .WithMessage(ValidationMessageProvider.CannotBeNegative("story points"))
                .When(r => r.StoryPoints.HasValue);

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