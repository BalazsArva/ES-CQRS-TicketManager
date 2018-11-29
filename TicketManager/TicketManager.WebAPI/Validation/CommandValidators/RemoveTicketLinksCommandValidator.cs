﻿using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketLinksCommandValidator : TicketCommandValidatorBase<RemoveTicketLinksCommand>
    {
        public RemoveTicketLinksCommandValidator(IEventsContextFactory eventsContextFactory, TicketLinkValidator ticketLinkValidator)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("modifier"));

            RuleFor(cmd => cmd.SourceTicketId)
                .Must(TicketExists)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("source ticket"));

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        protected override ISet<int> ExtractReferencedTicketIds(ValidationContext<RemoveTicketLinksCommand> context)
        {
            var command = context.InstanceToValidate;

            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.SourceTicketId }).ToHashSet();
        }
    }
}