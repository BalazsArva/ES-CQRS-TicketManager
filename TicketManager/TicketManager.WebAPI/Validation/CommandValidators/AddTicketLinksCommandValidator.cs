using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;
using TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AddTicketLinksCommandValidator : TicketCommandValidatorBase<AddTicketLinksCommand>
    {
        private readonly IDocumentStore documentStore;

        public AddTicketLinksCommandValidator(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore, TicketLinkValidator_AddLinks ticketLinkValidator)
            : base(eventsContextFactory)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

            RuleFor(cmd => cmd.Links)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyCollection("link"))
                .WithErrorCode(ValidationErrorCodes.BadRequest)
                .DependentRules(() =>
                {
                    RuleForEach(cmd => cmd.Links)
                        .SetValidator(ticketLinkValidator);
                });
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<AddTicketLinksCommand> context, CancellationToken cancellationToken)
        {
            context.RootContextData[ValidationContextKeys.TicketLinkOperationCommandContextDataKey] = context.InstanceToValidate;
            context.RootContextData[ValidationContextKeys.FoundTicketLinksContextDataKey] = await LinkValidationHelper
                .GetAssignedLinksAsync(documentStore, context.InstanceToValidate, cancellationToken)
                .ConfigureAwait(false);

            return await base.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
        }

        protected override ISet<long> ExtractReferencedTicketIds(AddTicketLinksCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }
    }
}