using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Raven.Client.Documents;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class CreateTicketCommandValidator : ValidatorBase<CreateTicketCommand>
    {
        protected readonly IDocumentStore documentStore;

        public CreateTicketCommandValidator(IDocumentStore documentStore, TicketLinkValidator_CreateInitialLinks ticketLinkValidator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

            RuleFor(cmd => cmd.RaisedByUser)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("creator"));

            RuleFor(cmd => cmd.Title)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("title"));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketPriorities>("priority"));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketTypes>("ticket type"));

            RuleFor(cmd => cmd.TicketStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketTypes>("ticket status"));

            // TODO: Review WithErrorCode everywhere
            RuleForEach(cmd => cmd.Tags)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"))
                .WithErrorCode(ValidationErrorCodes.BadRequest);

            RuleForEach(cmd => cmd.Tags)
                .Must(TagValidationHelper.BeAUniqueTag)
                .WithMessage("This tag is being added multiple times.")
                .WithErrorCode(ValidationErrorCodes.Conflict);

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<CreateTicketCommand> context, CancellationToken cancellationToken)
        {
            context.RootContextData[ValidationContextKeys.CreateTicketCommandContextDataKey] = context.InstanceToValidate;

            return base.ValidateAsync(context, cancellationToken);
        }
    }
}