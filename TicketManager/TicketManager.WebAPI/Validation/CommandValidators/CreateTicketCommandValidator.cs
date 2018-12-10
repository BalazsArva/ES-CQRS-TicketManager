using FluentValidation;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class CreateTicketCommandValidator : ValidatorBase<CreateTicketCommand>
    {
        public CreateTicketCommandValidator()
        {
            RuleFor(cmd => cmd.RaisedByUser)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("creator"));

            RuleFor(cmd => cmd.Title)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("title"));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketPriorities>("priority"));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketTypes>("ticket type"));

            RuleFor(cmd => cmd.TicketStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketTypes>("ticket status"));
        }
    }
}