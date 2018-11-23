using FluentValidation;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation
{
    public class AddTicketTagCommandValidator : AbstractValidator<AddTicketTagCommand>
    {
        public AddTicketTagCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the Ticket.

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AddTicketTagCommand.User)));

            RuleFor(cmd => cmd.Tag)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AddTicketTagCommand.Tag)));
        }
    }
}