using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketStatusCommandValidator : TicketCommandValidatorBase<ChangeTicketStatusCommand>
    {
        public ChangeTicketStatusCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("modifier"));

            RuleFor(cmd => cmd.NewStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>("new status"));

            RuleFor(cmd => cmd.TicketId)
                .Must(TicketExists)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(ValidationContext<ChangeTicketStatusCommand> context)
        {
            return new HashSet<int>
            {
                context.InstanceToValidate.TicketId
            };
        }
    }
}