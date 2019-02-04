using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators.Abstractions
{
    public abstract class TicketLinkDTOValidatorBase : AbstractValidator<TicketLinkDTO>
    {
        protected TicketLinkDTOValidatorBase()
        {
            RuleFor(link => link)
                .Must((link, _, context) =>
                {
                    var parent = (ILinkOperationCommand)context.ParentContext.RootContextData[ValidationContextKeys.TicketLinkOperationCommandContextDataKey];
                    var numberOfLinksWithSameProperties = parent.Links.Count(x => x.TargetTicketId == link.TargetTicketId && x.LinkType == link.LinkType);

                    return numberOfLinksWithSameProperties == 1;
                })
                .WithMessage("This link is being added multiple times.");

            RuleFor(link => link.LinkType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketLinkTypes>("link type"));

            RuleFor(link => link.TargetTicketId)
                .Must((link, targetTicketId, context) =>
                {
                    var parent = (ILinkOperationCommand)context.ParentContext.RootContextData[ValidationContextKeys.TicketLinkOperationCommandContextDataKey];

                    return targetTicketId != parent.TicketId;
                })
                .WithMessage("A ticket cannot be linked to itself.");

            RuleFor(link => link.TargetTicketId)
                .Must((link, targetTicketId, context) =>
                {
                    if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] is ISet<long> foundTicketIds)
                    {
                        return foundTicketIds.Contains(targetTicketId);
                    }

                    throw new InvalidOperationException(
                        "The validation could not be performed because the collection of existing ticket identifiers was not found in the context data of the validation context.");
                })
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("link"));
        }
    }
}