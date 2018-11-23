using FluentValidation;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation
{
    public class EditTicketDetailsCommandValidator : AbstractValidator<EditTicketDetailsCommand>
    {
        public EditTicketDetailsCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the Ticket.

            RuleFor(cmd => cmd.Editor)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(EditTicketDetailsCommand.Editor)));

            RuleFor(cmd => cmd.Title)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(EditTicketDetailsCommand.Title)));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>(nameof(EditTicketDetailsCommand.Priority)));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketType>(nameof(EditTicketDetailsCommand.TicketType)));
        }
    }
}