using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

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