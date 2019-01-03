using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class CancelTicketInvolvementCommandValidator : TicketCommandValidatorBase<CancelTicketInvolvementCommand>
    {
        public CancelTicketInvolvementCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.CancelInvolvementFor)
                // TODO: Write a custom rule method "BeAValidUser"
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("involved user"));
        }
    }
}