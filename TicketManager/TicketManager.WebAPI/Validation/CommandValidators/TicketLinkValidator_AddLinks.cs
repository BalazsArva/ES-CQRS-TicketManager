using System;
using System.Collections.Generic;
using FluentValidation;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class TicketLinkValidator_AddLinks : TicketLinkDTOValidatorBase
    {
        public TicketLinkValidator_AddLinks(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(link => link)
                .Must((link, _, context) =>
                {
                    if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketLinksContextDataKey] is ISet<TicketLink> assignedLinkSet)
                    {
                        return !assignedLinkSet.Contains(new TicketLink
                        {
                            LinkType = link.LinkType,
                            TargetTicketId = documentStore.GeneratePrefixedDocumentId<Ticket>(link.TargetTicketId)
                        });
                    }

                    throw new InvalidOperationException(
                        "The validation could not be performed because the collection of assigned links was not found in the validation context data.");
                })
                .WithMessage(ValidationMessageProvider.MustNotBeAnAssignedLink())
                .WithErrorCode(ValidationErrorCodes.Conflict);
        }
    }
}