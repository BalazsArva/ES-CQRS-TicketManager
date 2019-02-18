using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Raven.Client.Documents;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.EntityFramework.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.ValidationHelpers;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class CreateTicketCommandValidator : ValidatorBase<CreateTicketCommand>
    {
        protected readonly IDocumentStore documentStore;
        protected readonly IEventsContextFactory eventsContextFactory;

        public CreateTicketCommandValidator(IDocumentStore documentStore, IEventsContextFactory eventsContextFactory, TicketLinkValidator_CreateInitialLinks ticketLinkValidator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));

            RuleFor(cmd => cmd.RaisedByUser)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("creator"));

            RuleFor(cmd => cmd.Title)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("title"));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketPriorities>("priority"));

            RuleFor(cmd => cmd.Type)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketTypes>("ticket type"));

            RuleFor(cmd => cmd.Status)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketTypes>("ticket status"));

            RuleFor(cmd => cmd.StoryPoints)
                .GreaterThanOrEqualTo(ValidationConstants.MinStoryPoints)
                .WithMessage(ValidationMessageProvider.CannotBeNegative("story points"));

            RuleForEach(cmd => cmd.Tags)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"));

            RuleForEach(cmd => cmd.Tags)
                .Must(TagValidationHelper.BeAUniqueTag)
                .WithMessage("This tag is being added multiple times.");

            RuleForEach(cmd => cmd.Links)
                .SetValidator(ticketLinkValidator);
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<CreateTicketCommand> context, CancellationToken cancellationToken)
        {
            context.RootContextData[ValidationContextKeys.CreateTicketCommandContextDataKey] = context.InstanceToValidate;

            using (var ctx = eventsContextFactory.CreateContext())
            {
                var requiredTicketIds = ExtractReferencedTicketIds(context.InstanceToValidate);
                var foundTickets = await ctx
                    .TicketCreatedEvents
                    .Where(evt => requiredTicketIds.Contains(evt.Id))
                    .Select(evt => evt.Id)
                    .ToSetAsync(cancellationToken)
                    .ConfigureAwait(false);

                context.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] = foundTickets;
            }

            return await base.ValidateAsync(context, cancellationToken);
        }

        private ISet<long> ExtractReferencedTicketIds(CreateTicketCommand command)
        {
            return new HashSet<long>(command.Links?.Select(lnk => lnk.TargetTicketId) ?? Enumerable.Empty<long>());
        }
    }
}