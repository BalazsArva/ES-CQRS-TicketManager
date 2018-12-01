using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AddTicketLinksCommandValidator : TicketCommandValidatorBase<AddTicketLinksCommand>
    {
        public AddTicketLinksCommandValidator(IEventsContextFactory eventsContextFactory, IValidator<TicketLinkDTO> ticketLinkValidator)
            : base(eventsContextFactory)
        {
            RuleForEach(cmd => cmd.Links)
                .Must((command, link) => link.TargetTicketId != command.TicketId)
                .WithMessage("A ticket cannot be linked to itself.");

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        protected override ISet<int> ExtractReferencedTicketIds(AddTicketLinksCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }
    }
}