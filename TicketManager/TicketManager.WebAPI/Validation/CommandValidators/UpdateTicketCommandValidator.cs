﻿using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class UpdateTicketCommandValidator : TicketCommandValidatorBase<UpdateTicketCommand>
    {
        public UpdateTicketCommandValidator(IEventsContextFactory eventsContextFactory, TicketLinkValidator ticketLinkValidator)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));

            RuleFor(cmd => cmd.Title)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("title"));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>("priority"));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketType>("ticket type"));

            RuleFor(cmd => cmd.TicketStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketStatus>("ticket status"));

            RuleFor(cmd => cmd.User)
                .Must(user => !string.IsNullOrWhiteSpace(user))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));

            RuleForEach(cmd => cmd.Links)
                .Must((command, link) => link.TargetTicketId != command.TicketId)
                .WithMessage("A ticket cannot be linked to itself.");

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);

            RuleForEach(cmd => cmd.Tags)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(UpdateTicketCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }
    }
}