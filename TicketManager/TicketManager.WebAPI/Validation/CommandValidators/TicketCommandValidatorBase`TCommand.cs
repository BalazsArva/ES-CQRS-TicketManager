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
    public abstract class TicketCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    {
        protected readonly IEventsContextFactory eventsContextFactory;

        protected TicketCommandValidatorBase(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<TCommand> context, CancellationToken cancellationToken = default)
        {
            context.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] = await FindExistingReferencedTicketIdsAsync(context.InstanceToValidate, cancellationToken);

            return await base.ValidateAsync(context, cancellationToken);
        }

        protected bool TicketExists(TCommand command, int ticketId, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] is ISet<int> foundTicketIds)
            {
                return foundTicketIds.Contains(ticketId);
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of existing ticket identifiers was not found in the context data of the validation context.");
        }

        /// <summary>
        /// Asynchronously returns a set which contains all ticket Ids which can be found in any appropriate property of
        /// the validated object and which are verified to exist.
        /// </summary>
        /// <param name="command">
        /// The command to find the existing ticket Ids for.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A set which contains all ticket ids which are referenced in any property of the validated object and verified to exist.
        /// </returns>
        protected virtual async Task<ISet<int>> FindExistingReferencedTicketIdsAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            var requiredTicketIds = ExtractReferencedTicketIds(command);

            using (var context = eventsContextFactory.CreateContext())
            {
                return await context
                    .TicketCreatedEvents
                    .Where(evt => requiredTicketIds.Contains(evt.Id))
                    .Select(evt => evt.Id)
                    .ToSetAsync();
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns a set which contains all ticket Ids which can be found in any
        /// appropriate property of the validated object. Override this method to collect all ticket Ids from all
        /// properties to support batch retrievals from data sources rather than quering them one-by-one for each
        /// item/property which concerns a ticket. The existence check can later be performed by retrieving the result of
        /// this method which is stored in the <see cref="ValidationContext.RootContextData"/> with the key <see cref="ValidationContextKeys.FoundTicketIdsContextDataKey"/>.
        /// </summary>
        /// <param name="command">
        /// The command to extract referenced ticket Ids from.
        /// </param>
        /// <returns>
        /// A set which contains all ticket ids which are referenced in any property of the validated object.
        /// </returns>
        protected abstract ISet<int> ExtractReferencedTicketIds(TCommand command);
    }
}