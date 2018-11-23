using FluentValidation;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation
{
    public class AddTicketLinkCommandValidator : AbstractValidator<AddTicketLinkCommand>
    {
        public AddTicketLinkCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the Source and Target Ticket.

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AddTicketLinkCommand.User)));

            RuleFor(cmd => cmd.LinkType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<LinkType>(nameof(AddTicketLinkCommand.LinkType)));
        }
    }
}