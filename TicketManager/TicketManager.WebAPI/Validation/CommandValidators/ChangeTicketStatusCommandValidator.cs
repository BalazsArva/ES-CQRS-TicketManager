﻿using System.Collections.Generic;
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
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));

            RuleFor(cmd => cmd.NewStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>("new status"));

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(ChangeTicketStatusCommand command)
        {
            return new HashSet<int> { command.TicketId };
        }
    }
}