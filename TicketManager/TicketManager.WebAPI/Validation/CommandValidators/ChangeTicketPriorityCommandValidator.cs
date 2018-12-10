using FluentValidation;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketPriorityCommandValidator : TicketCommandValidatorBase<ChangeTicketPriorityCommand>
    {
        public ChangeTicketPriorityCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketPriorities>("priority"));
        }
    }
}