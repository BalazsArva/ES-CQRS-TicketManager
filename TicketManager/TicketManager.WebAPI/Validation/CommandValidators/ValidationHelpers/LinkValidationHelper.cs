using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Validators;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers
{
    public static class LinkValidationHelper
    {
        public static bool BeAUniqueLink(ILinkOperationCommand command, TicketLinkDTO link, PropertyValidatorContext context)
        {
            return command.Links.Count(otherLink => otherLink.LinkType == link.LinkType && otherLink.TargetTicketId == link.TargetTicketId) == 1;
        }

        public static async Task<ISet<TicketLink>> GetAssignedLinksAsync<TCommand>(IDocumentStore documentStore, TCommand command, CancellationToken cancellationToken)
            where TCommand : TicketCommandBase, ILinkOperationCommand
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(command.TicketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                return new HashSet<TicketLink>(ticketDocument?.Links?.LinkSet ?? Enumerable.Empty<TicketLink>());
            }
        }

        public static bool ReferenceAnExistingTicket(ILinkOperationCommand command, TicketLinkDTO link, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] is ISet<long> foundTicketIds)
            {
                return foundTicketIds.Contains(link.TargetTicketId);
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of existing ticket identifiers was not found in the context data of the validation context.");
        }
    }
}