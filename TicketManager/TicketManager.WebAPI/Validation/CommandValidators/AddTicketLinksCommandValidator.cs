using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AddTicketLinksCommandValidator : TicketCommandValidatorBase<AddTicketLinksCommand>
    {
        public AddTicketLinksCommandValidator(IEventsContextFactory eventsContextFactory, TicketLinkValidator ticketLinkValidator)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.User)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));

            RuleFor(cmd => cmd.SourceTicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("source ticket"));

            RuleForEach(cmd => cmd.Links)
                .Must((command, link) => link.TargetTicketId != command.SourceTicketId)
                .WithMessage("A ticket cannot be linked to itself.");

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        protected override ISet<int> ExtractReferencedTicketIds(AddTicketLinksCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.SourceTicketId }).ToHashSet();
        }
    }
}