using FluentValidation;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketTypeCommandValidator : TicketCommandValidatorBase<ChangeTicketTypeCommand>
    {
        public ChangeTicketTypeCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketPriorities>("ticket type"));
        }
    }
}