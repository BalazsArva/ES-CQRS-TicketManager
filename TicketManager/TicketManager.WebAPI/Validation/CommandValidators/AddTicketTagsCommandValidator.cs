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
    public class AddTicketTagsCommandValidator : TicketCommandValidatorBase<AddTicketTagsCommand>
    {
        private readonly IDocumentStore documentStore;

        public AddTicketTagsCommandValidator(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
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
                        .Must(NotBeWhitespaceOnly)
                        .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"))
                        .WithErrorCode(ValidationErrorCodes.BadRequest);

                    RuleForEach(cmd => cmd.Tags)
                        .Must(TagValidationHelper.BeAUniqueTag)
                        .WithMessage("This tag is being added multiple times.")
                        .WithErrorCode(ValidationErrorCodes.Conflict);

                    RuleForEach(cmd => cmd.Tags)
                        .Must(NotBeAnAssignedTag)
                        .WithMessage(ValidationMessageProvider.MustNotBeAnAssignedTag("tag"))
                        .WithErrorCode(ValidationErrorCodes.Conflict);
                });
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<AddTicketTagsCommand> context, CancellationToken cancellationToken = default)
        {
            context.RootContextData[ValidationContextKeys.FoundTicketTagsContextDataKey] = await TagValidationHelper.GetAssignedTagsAsync(documentStore, context.InstanceToValidate);

            return await base.ValidateAsync(context, cancellationToken);
        }

        private bool NotBeAnAssignedTag(AddTicketTagsCommand command, string tag, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketTagsContextDataKey] is ISet<string> assignedTagSet)
            {
                return !assignedTagSet.Contains(tag);
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of assigned tags was not found in the validation context data.");
        }
    }
}