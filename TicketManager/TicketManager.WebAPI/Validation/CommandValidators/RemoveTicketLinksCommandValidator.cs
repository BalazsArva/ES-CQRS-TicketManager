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
            RuleFor(cmd => cmd.RaisedByUser)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("source ticket"));

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        protected override ISet<int> ExtractReferencedTicketIds(RemoveTicketLinksCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }
    }
}