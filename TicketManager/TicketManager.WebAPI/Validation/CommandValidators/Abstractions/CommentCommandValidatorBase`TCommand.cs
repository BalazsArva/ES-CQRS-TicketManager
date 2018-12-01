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
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators.Abstractions
{
    public abstract class CommentCommandValidatorBase<TCommand> : ValidatorBase<TCommand>
        where TCommand : ICommentCommand
    {
        protected readonly IEventsContextFactory eventsContextFactory;

        protected CommentCommandValidatorBase(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));

            RuleFor(cmd => cmd.CommentId)
                .Must(BeAnExistingComment)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingComment("comment"));

            RuleFor(cmd => cmd.RaisedByUser)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<TCommand> context, CancellationToken cancellationToken = default)
        {
            context.RootContextData[ValidationContextKeys.FoundCommentIdsContextDataKey] = await FindExistingReferencedCommentIdsAsync(context.InstanceToValidate, cancellationToken);

            return await base.ValidateAsync(context, cancellationToken);
        }

        protected bool BeAnExistingComment(TCommand command, long commentId, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundCommentIdsContextDataKey] is ISet<long> foundCommentIds)
            {
                return foundCommentIds.Contains(commentId);
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of existing comment identifiers was not found in the validation context data.");
        }

        /// <summary>
        /// Asynchronously returns a set which contains all comment Ids which can be found in any appropriate property of
        /// the validated object and which are verified to exist.
        /// </summary>
        /// <param name="command">
        /// The command to find the existing comment Ids for.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A set which contains all comment ids which are referenced in any property of the validated object and verified to exist.
        /// </returns>
        protected virtual async Task<ISet<long>> FindExistingReferencedCommentIdsAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            var requiredCommentIds = ExtractReferencedCommentIds(command);

            using (var context = eventsContextFactory.CreateContext())
            {
                return await context
                    .TicketCommentPostedEvents
                    .Where(evt => requiredCommentIds.Contains(evt.Id))
                    .Select(evt => evt.Id)
                    .ToSetAsync();
            }
        }

        /// <summary>
        /// Returns a set which contains all comment Ids which can be found in any appropriate property of the validated
        /// object. If the <typeparamref name="TCommand"/> type contains properties other than <see
        /// cref="ICommentCommand.CommentId"/> which represents a comment, then override this method to collect all comment
        /// Ids from all appropriate properties to support batch retrievals from data sources rather than quering them
        /// one-by-one for each item/property. The existence check can later be performed by retrieving the result of
        /// this method which is stored in the <see cref="ValidationContext.RootContextData"/> with the key <see cref="ValidationContextKeys.FoundCommentIdsContextDataKey"/>.
        /// </summary>
        /// <param name="command">
        /// The command to extract referenced comment Ids from.
        /// </param>
        /// <returns>
        /// A set which contains all comment ids which are referenced in any property of the validated object.
        /// </returns>
        protected virtual ISet<long> ExtractReferencedCommentIds(TCommand command)
        {
            return new HashSet<long> { command.CommentId };
        }
    }
}