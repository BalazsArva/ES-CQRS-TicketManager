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
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("modifier"));

            RuleFor(cmd => cmd.SourceTicketId)
                .Must(TicketExists)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("source ticket"));

            RuleForEach(cmd => cmd.Links)
               .Must((command, link) => link.TargetTicketId != command.SourceTicketId)
               .WithMessage("A ticket link cannot be established to the same ticket.");

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        protected override ISet<int> ExtractReferencedTicketIds(ValidationContext<AddTicketLinksCommand> context)
        {
            var command = context.InstanceToValidate;

            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.SourceTicketId }).ToHashSet();
        }
    }
}