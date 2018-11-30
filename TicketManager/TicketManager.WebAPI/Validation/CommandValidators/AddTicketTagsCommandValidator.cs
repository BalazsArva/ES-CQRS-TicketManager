using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AddTicketTagsCommandValidator : TicketCommandValidatorBase<AddTicketTagsCommand>
    {
        public AddTicketTagsCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.User)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));

            RuleForEach(cmd => cmd.Tags)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"));

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(AddTicketTagsCommand command)
        {
            return new HashSet<int> { command.TicketId };
        }
    }
}