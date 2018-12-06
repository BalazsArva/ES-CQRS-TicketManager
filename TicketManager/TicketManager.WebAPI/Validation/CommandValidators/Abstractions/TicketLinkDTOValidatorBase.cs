using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Raven.Client.Documents;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators.Abstractions
{
    public abstract class TicketLinkDTOValidatorBase : AbstractValidator<TicketLinkDTO>
    {
        protected readonly IDocumentStore documentStore;

        protected TicketLinkDTOValidatorBase(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

            RuleFor(link => link)
                .Must((link, _, context) =>
                {
                    var parent = (ILinkOperationCommand)context.ParentContext.RootContextData[ValidationContextKeys.TicketLinkOperationCommandContextDataKey];
                    var numberOfLinksWithSameProperties = parent.Links.Count(x => x.TargetTicketId == link.TargetTicketId && x.LinkType == link.LinkType);

                    return numberOfLinksWithSameProperties == 1;
                })
                .WithMessage("This link is being added multiple times.")
                .WithErrorCode(ValidationErrorCodes.Conflict);

            RuleFor(link => link.LinkType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketLinkTypes>("link type"));

            RuleFor(link => link.TargetTicketId)
                .Must((link, targetTicketId, context) =>
                {
                    var parent = (ILinkOperationCommand)context.ParentContext.RootContextData[ValidationContextKeys.TicketLinkOperationCommandContextDataKey];

                    return targetTicketId != parent.TicketId;
                })
                .WithMessage("A ticket cannot be linked to itself.")
                .WithErrorCode(ValidationErrorCodes.BadRequest);

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
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("link"))
                .WithErrorCode(ValidationErrorCodes.NotFound);
        }
    }
}