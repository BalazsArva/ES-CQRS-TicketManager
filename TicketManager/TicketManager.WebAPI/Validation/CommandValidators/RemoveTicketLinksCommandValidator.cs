using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;
using TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class RemoveTicketLinksCommandValidator : TicketCommandValidatorBase<RemoveTicketLinksCommand>
    {
        private readonly IDocumentStore documentStore;

        public RemoveTicketLinksCommandValidator(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

            RuleFor(cmd => cmd.Links)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyCollection("link"))
                .DependentRules(() =>
                {
                    RuleForEach(cmd => cmd.Links)
                        .Must(LinkValidationHelper.BeAUniqueLink)
                        .WithMessage("This link is being removed multiple times.");

                    RuleForEach(cmd => cmd.Links)
                        .Must(BeAnExistingLink)
                        .WithMessage(ValidationMessageProvider.MustNotBeAnAssignedLink());
                });
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<RemoveTicketLinksCommand> context, CancellationToken cancellationToken)
        {
            context.RootContextData[ValidationContextKeys.FoundTicketLinksContextDataKey] = await LinkValidationHelper
                .GetAssignedLinksAsync(documentStore, context.InstanceToValidate, cancellationToken)
                .ConfigureAwait(false);

            return await base.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
        }

        protected override ISet<long> ExtractReferencedTicketIds(RemoveTicketLinksCommand command)
        {
            return command.Links.Select(link => link.TargetTicketId).Concat(new[] { command.TicketId }).ToHashSet();
        }

        private bool BeAnExistingLink(RemoveTicketLinksCommand command, TicketLinkDTO link, PropertyValidatorContext context)
        {
            if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketLinksContextDataKey] is ISet<TicketLink> assignedLinkSet)
            {
                return assignedLinkSet.Contains(new TicketLink
                {
                    LinkType = link.LinkType,
                    TargetTicketId = documentStore.GeneratePrefixedDocumentId<Ticket>(link.TargetTicketId)
                });
            }

            throw new InvalidOperationException(
                "The validation could not be performed because the collection of assigned links was not found in the validation context data.");
        }
    }
}