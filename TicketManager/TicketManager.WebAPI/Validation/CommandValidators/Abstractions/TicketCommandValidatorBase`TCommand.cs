using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using TicketManager.DataAccess.EntityFramework.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators.Abstractions
{
    public abstract class TicketCommandValidatorBase<TCommand> : ValidatorBase<TCommand>
        where TCommand : ITicketCommand
    {
        protected readonly IEventsContextFactory eventsContextFactory;

        protected TicketCommandValidatorBase(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));

            RuleFor(cmd => cmd.RaisedByUser)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("modifier"));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<TCommand> context, CancellationToken cancellationToken)
        {
            context.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] = await FindExistingReferencedTicketIdsAsync(context.InstanceToValidate, cancellationToken).ConfigureAwait(false);

            return await base.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
        }

        protected bool BeAnExistingTicket(TCommand command, long ticketId, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] is ISet<long> foundTicketIds)
            {
                return foundTicketIds.Contains(ticketId);
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of existing ticket identifiers was not found in the validation context data.");
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
        protected virtual async Task<ISet<long>> FindExistingReferencedTicketIdsAsync(TCommand command, CancellationToken cancellationToken)
        {
            var requiredTicketIds = ExtractReferencedTicketIds(command);

            using (var context = eventsContextFactory.CreateContext())
            {
                return await context
                    .TicketCreatedEvents
                    .Where(evt => requiredTicketIds.Contains(evt.Id))
                    .Select(evt => evt.Id)
                    .ToSetAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns a set which contains all ticket Ids which can be found in any appropriate property of the validated
        /// object. If the <typeparamref name="TCommand"/> type contains properties other than <see
        /// cref="ITicketCommand.TicketId"/> which represents a ticket, then override this method to collect all ticket
        /// Ids from all appropriate properties to support batch retrievals from data sources rather than quering them
        /// one-by-one for each item/property. The existence check can later be performed by retrieving the result of
        /// this method which is stored in the <see cref="ValidationContext.RootContextData"/> with the key <see cref="ValidationContextKeys.FoundTicketIdsContextDataKey"/>.
        /// </summary>
        /// <param name="command">
        /// The command to extract referenced ticket Ids from.
        /// </param>
        /// <returns>
        /// A set which contains all ticket ids which are referenced in any property of the validated object.
        /// </returns>
        protected virtual ISet<long> ExtractReferencedTicketIds(TCommand command)
        {
            return new HashSet<long> { command.TicketId };
        }
    }
}