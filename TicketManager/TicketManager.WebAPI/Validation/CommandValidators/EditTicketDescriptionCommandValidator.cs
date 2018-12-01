using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class EditTicketDescriptionCommandValidator : TicketCommandValidatorBase<EditTicketDescriptionCommand>
    {
        public EditTicketDescriptionCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
        }
    }
}