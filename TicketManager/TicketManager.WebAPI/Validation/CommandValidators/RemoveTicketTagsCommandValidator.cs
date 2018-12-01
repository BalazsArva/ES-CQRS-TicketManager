using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketTagsCommandValidator : TicketCommandValidatorBase<RemoveTicketTagsCommand>
    {
        public RemoveTicketTagsCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            // TODO: Verify  that the tag is actually added (or maybe can ignore that).

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));

            RuleFor(cmd => cmd.RaisedByUser)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));

            RuleForEach(cmd => cmd.Tags)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(RemoveTicketTagsCommand command)
        {
            return new HashSet<int> { command.TicketId };
        }
    }
}