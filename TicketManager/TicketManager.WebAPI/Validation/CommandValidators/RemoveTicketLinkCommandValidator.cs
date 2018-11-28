using FluentValidation;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketLinkCommandValidator : AbstractValidator<RemoveTicketLinkCommand>
    {
        public RemoveTicketLinkCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the link.

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("modifier"));

            RuleFor(cmd => cmd.LinkType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<LinkType>("link type"));
        }
    }
}