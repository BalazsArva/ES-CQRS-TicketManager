using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Raven.Client.Documents;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;
using TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class UpdateTicketCommandValidator : TicketCommandValidatorBase<UpdateTicketCommand>
    {
        private readonly IDocumentStore documentStore;

        public UpdateTicketCommandValidator(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore, TicketLinkValidator_UpdateLinks ticketLinkValidator)
            : base(eventsContextFactory)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

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
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketStatuses>("ticket status"));

            RuleFor(cmd => cmd.StoryPoints)
                .GreaterThanOrEqualTo(ValidationConstants.MinStoryPoints)
                .WithMessage(ValidationMessageProvider.CannotBeNegative("story points"));

            RuleForEach(cmd => cmd.Links)
                .Must((command, link) => link.TargetTicketId != command.TicketId)
                .WithMessage("A ticket cannot be linked to itself.");

            // Unlike in Add/RemoveCommandValidator, here we can allow nulls because in Add/Remove, the list of tags
            // means the difference (i.e. what to add or remove), but here it's a complete replacement. It is acceptable
            // to erase all the tags.
            When(
                cmd => cmd.Tags != null && cmd.Tags.Length > 0,
                () =>
                {
                    RuleForEach(cmd => cmd.Tags)
                        .Must(NotBeWhitespaceOnly)
                        .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"));

                    RuleForEach(cmd => cmd.Tags)
                        .Must(TagValidationHelper.BeAUniqueTag)
                        .WithMessage("This tag is being removed multiple times.")
                        .WithErrorCode(ValidationErrorCodes.Conflict);
                });

            When(
                cmd => cmd.Links != null && cmd.Links.Length > 0,
                () => RuleForEach(cmd => cmd.Links).SetValidator(ticketLinkValidator));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<UpdateTicketCommand> context, CancellationToken cancellationToken)
        {
            context.RootContextData[ValidationContextKeys.TicketLinkOperationCommandContextDataKey] = context.InstanceToValidate;
            context.RootContextData[ValidationContextKeys.FoundTicketTagsContextDataKey] = await TagValidationHelper
                .GetAssignedTagsAsync(documentStore, context.InstanceToValidate, cancellationToken)
                .ConfigureAwait(false);
            context.RootContextData[ValidationContextKeys.FoundTicketLinksContextDataKey] = await LinkValidationHelper
                .GetAssignedLinksAsync(documentStore, context.InstanceToValidate, cancellationToken)
                .ConfigureAwait(false);

            return await base.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
        }

        protected override ISet<long> ExtractReferencedTicketIds(UpdateTicketCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }
    }
}