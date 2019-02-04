using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Raven.Client.Documents;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class TicketLinkValidator_CreateInitialLinks : AbstractValidator<TicketLinkDTO>
    {
        protected readonly IDocumentStore documentStore;

        public TicketLinkValidator_CreateInitialLinks(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

            RuleFor(link => link)
                .Must((link, _, context) =>
                {
                    var parent = (CreateTicketCommand)context.ParentContext.RootContextData[ValidationContextKeys.CreateTicketCommandContextDataKey];
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