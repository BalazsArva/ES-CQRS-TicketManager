using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AssignTicketCommandValidator : TicketCommandValidatorBase<AssignTicketCommand>
    {
        public AssignTicketCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.Assigner)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("modifier"));

            RuleFor(cmd => cmd.TicketId)
                .Must(TicketExists)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(AssignTicketCommand command)
        {
            return new HashSet<int>
            {
                command.TicketId
            };
        }
    }
}