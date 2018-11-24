using FluentValidation;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketTagCommandValidator : AbstractValidator<RemoveTicketTagCommand>
    {
        public RemoveTicketTagCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the Ticket and that the tag is actually added (actually can ignore that).

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(RemoveTicketTagCommand.User)));

            RuleFor(cmd => cmd.Tag)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(RemoveTicketTagCommand.Tag)));
        }
    }
}