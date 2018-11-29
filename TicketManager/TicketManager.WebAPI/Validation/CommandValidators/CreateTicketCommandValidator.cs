using FluentValidation;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
    {
        public CreateTicketCommandValidator()
        {
            RuleFor(cmd => cmd.Creator)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("creator"));

            RuleFor(cmd => cmd.Title)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("title"));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>("priority"));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketType>("ticket type"));
        }
    }
}