using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Validators;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers
{
    public static class TagValidationHelper
    {
        public static bool BeAUniqueTag(ITagOperationCommand command, string tag, PropertyValidatorContext context)
        {
            return command.Tags.Count(t => t == tag) == 1;
        }

        public static async Task<ISet<string>> GetAssignedTagsAsync<TCommand>(IDocumentStore documentStore, TCommand command)
            where TCommand : TicketCommandBase, ITagOperationCommand
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(command.TicketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                return new HashSet<string>(ticketDocument?.Tags?.TagSet ?? Enumerable.Empty<string>());
            }
        }
    }
}