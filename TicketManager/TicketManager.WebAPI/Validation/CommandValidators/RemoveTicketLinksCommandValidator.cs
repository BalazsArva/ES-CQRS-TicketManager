using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketLinksCommandValidator : TicketCommandValidatorBase<RemoveTicketLinksCommand>
    {
        public RemoveTicketLinksCommandValidator(IEventsContextFactory eventsContextFactory, IValidator<TicketLinkDTO> ticketLinkValidator)
            : base(eventsContextFactory)
        {
            // TODO: Verify  that the link is actually added (or maybe can ignore that).

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        protected override ISet<int> ExtractReferencedTicketIds(RemoveTicketLinksCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }
    }
}