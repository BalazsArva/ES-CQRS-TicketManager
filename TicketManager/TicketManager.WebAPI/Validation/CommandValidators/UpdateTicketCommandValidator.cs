using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;
using TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class UpdateTicketCommandValidator : TicketCommandValidatorBase<UpdateTicketCommand>
    {
        private readonly IDocumentStore documentStore;

        public UpdateTicketCommandValidator(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore, IValidator<TicketLinkDTO> ticketLinkValidator)
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
                () =>
                {
                    // TODO: This is not completely correct because the ticketLinkValidator does a check whether a link with same source, target and type already exists. That is not correct here, because the Links array does not represent a changeset here but an array of replacements.
                    RuleForEach(cmd => cmd.Links).SetValidator(ticketLinkValidator);
                });
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<UpdateTicketCommand> context, CancellationToken cancellationToken = default)
        {
            context.RootContextData[ValidationContextKeys.TicketLinkOperationCommandContextDataKey] = context.InstanceToValidate;
            context.RootContextData[ValidationContextKeys.FoundTicketTagsContextDataKey] = await TagValidationHelper.GetAssignedTagsAsync(documentStore, context.InstanceToValidate);
            context.RootContextData[ValidationContextKeys.FoundTicketLinksContextDataKey] = await LinkValidationHelper.GetAssignedLinksAsync(documentStore, context.InstanceToValidate);

            return await base.ValidateAsync(context, cancellationToken);
        }

        protected override ISet<long> ExtractReferencedTicketIds(UpdateTicketCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }
    }
}