using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;
using TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketTagsCommandValidator : TicketCommandValidatorBase<RemoveTicketTagsCommand>
    {
        private readonly IDocumentStore documentStore;

        public RemoveTicketTagsCommandValidator(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

            RuleFor(cmd => cmd.Tags)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyCollection("tag"))
                .WithErrorCode(ValidationErrorCodes.BadRequest)
                .DependentRules(() =>
                {
                    RuleForEach(cmd => cmd.Tags)
                        .Must(TagValidationHelper.BeAUniqueTag)
                        .WithMessage("This tag is being removed multiple times.")
                        .WithErrorCode(ValidationErrorCodes.Conflict);

                    RuleForEach(cmd => cmd.Tags)
                        .Must(BeAnAssignedTag)
                        .WithMessage(ValidationMessageProvider.MustBeAnAssignedTag("tag"))
                        .WithErrorCode(ValidationErrorCodes.NotFound);
                });
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<RemoveTicketTagsCommand> context, CancellationToken cancellationToken)
        {
            context.RootContextData[ValidationContextKeys.FoundTicketTagsContextDataKey] = await TagValidationHelper.GetAssignedTagsAsync(documentStore, context.InstanceToValidate, cancellationToken).ConfigureAwait(false);

            return await base.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
        }

        private bool BeAnAssignedTag(RemoveTicketTagsCommand command, string tag, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketTagsContextDataKey] is ISet<string> assignedTagSet)
            {
                return assignedTagSet.Contains(tag);
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of assigned tags was not found in the validation context data.");
        }
    }
}