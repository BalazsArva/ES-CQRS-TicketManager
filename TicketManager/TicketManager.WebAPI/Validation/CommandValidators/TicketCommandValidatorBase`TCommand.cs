using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public abstract class TicketCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    {
        private readonly IDocumentStore documentStore;

        protected TicketCommandValidatorBase(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new System.ArgumentNullException(nameof(documentStore));
        }

        protected async Task<bool> TicketExistsAsync(int ticketId, CancellationToken cancellationToken)
        {
            // TODO: Consider replacing the session with query store when implemented.
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());

                return await session.Advanced.ExistsAsync(ticketDocumentId);
            }
        }

        protected async Task<bool> CommentExistsAsync(int commentId, CancellationToken cancellationToken)
        {
            // TODO: Consider replacing the session with query store when implemented.
            using (var session = documentStore.OpenAsyncSession())
            {
                var commentDocumentId = session.GeneratePrefixedDocumentId<Comment>(commentId.ToString());

                return await session.Advanced.ExistsAsync(commentDocumentId);
            }
        }
    }
}