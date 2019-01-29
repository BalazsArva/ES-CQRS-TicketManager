using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketDescriptionCommandValidator : TicketCommandValidatorBase<ChangeTicketDescriptionCommand>
    {
        public ChangeTicketDescriptionCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
        }
    }
}