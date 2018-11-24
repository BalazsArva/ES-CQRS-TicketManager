using FluentValidation;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketStatusCommandValidator : AbstractValidator<ChangeTicketStatusCommand>
    {
        public ChangeTicketStatusCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the Ticket.

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(ChangeTicketStatusCommand.User)));

            RuleFor(cmd => cmd.NewStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>(nameof(ChangeTicketStatusCommand.NewStatus)));
        }
    }
}