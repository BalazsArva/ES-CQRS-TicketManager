using FluentValidation;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketStatusCommandValidator : TicketCommandValidatorBase<ChangeTicketStatusCommand>
    {
        public ChangeTicketStatusCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.Status)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketPriorities>("new status"));
        }
    }
}