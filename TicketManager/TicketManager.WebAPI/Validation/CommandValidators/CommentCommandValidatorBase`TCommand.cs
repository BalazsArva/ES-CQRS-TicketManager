using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public abstract class CommentCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    {
        protected readonly IEventsContextFactory eventsContextFactory;

        protected CommentCommandValidatorBase(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<TCommand> context, CancellationToken cancellationToken = default)
        {
            context.RootContextData[ValidationContextKeys.FoundCommentIdsContextDataKey] = await FindExistingReferencedCommentIdsAsync(context, cancellationToken);

            return await base.ValidateAsync(context, cancellationToken);
        }

        protected bool CommentExists(TCommand command, int commentId, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundCommentIdsContextDataKey] is ISet<int> foundTicketIds)
            {
                return foundTicketIds.Contains(commentId);
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of existing comment identifiers was not found in the context data of the validation context.");
        }

        /// <summary>
        /// Asynchronously returns a set which contains all comment Ids which can be found in any appropriate property of
        /// the validated object and which are verified to exist.
        /// </summary>
        /// <param name="context">
        /// The validation context.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A set which contains all comment ids which are referenced in any property of the validated object and verified to exist.
        /// </returns>
        protected virtual async Task<ISet<int>> FindExistingReferencedCommentIdsAsync(ValidationContext<TCommand> context, CancellationToken cancellationToken = default)
        {
            var requiredCommentIds = ExtractReferencedCommentIds(context);

            using (var dbcontext = eventsContextFactory.CreateContext())
            {
                return await dbcontext
                    .TicketCommentPostedEvents
                    .Where(evt => requiredCommentIds.Contains(evt.Id))
                    .Select(evt => evt.Id)
                    .ToSetAsync();
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns a set which contains all comment Ids which can be found in any
        /// appropriate property of the validated object. Override this method to collect all comment Ids from all
        /// properties to support batch retrievals from data sources rather than quering them one-by-one for each
        /// item/property which concerns a comment. The existence check can later be performed by retrieving the result of
        /// this method which is stored in the <see cref="ValidationContext.RootContextData"/> with the key <see cref="ValidationContextKeys.FoundTicketIdsContextDataKey"/>.
        /// </summary>
        /// <param name="context">
        /// The validation context.
        /// </param>
        /// <returns>
        /// A set which contains all comment ids which are referenced in any property of the validated object.
        /// </returns>
        protected abstract ISet<int> ExtractReferencedCommentIds(ValidationContext<TCommand> context);
    }
}