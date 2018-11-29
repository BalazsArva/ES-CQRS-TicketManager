using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketTagCommandValidator : TicketCommandValidatorBase<RemoveTicketTagCommand>
    {
        public RemoveTicketTagCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            // TODO: Verify  that the tag is actually added (or maybe can ignore that).

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));

            RuleFor(cmd => cmd.User)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));

            RuleFor(cmd => cmd.Tag)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(RemoveTicketTagCommand command)
        {
            return new HashSet<int> { command.TicketId };
        }
    }
}