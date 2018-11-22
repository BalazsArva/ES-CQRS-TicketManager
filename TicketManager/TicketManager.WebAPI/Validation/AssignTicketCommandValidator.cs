using FluentValidation;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation
{
    public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
    {
        public AssignTicketCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the Ticket.

            RuleFor(cmd => cmd.Assigner)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AssignTicketCommand.Assigner)));
        }
    }
}