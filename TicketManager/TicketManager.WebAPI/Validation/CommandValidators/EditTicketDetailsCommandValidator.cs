using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class EditTicketDetailsCommandValidator : TicketCommandValidatorBase<EditTicketDetailsCommand>
    {
        public EditTicketDetailsCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.Editor)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("editor"));

            RuleFor(cmd => cmd.Title)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("title"));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>("priority"));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketType>("ticket type"));

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(EditTicketDetailsCommand command)
        {
            return new HashSet<int> { command.TicketId };
        }
    }
}