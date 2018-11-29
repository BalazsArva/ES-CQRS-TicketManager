using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class UpdateTicketCommandValidator : TicketCommandValidatorBase<UpdateTicketCommand>
    {
        private const string FoundTicketIdsContextDataKey = "FoundTicketIds";

        private readonly IEventsContextFactory eventsContextFactory;

        public UpdateTicketCommandValidator(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(documentStore)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new System.ArgumentNullException(nameof(eventsContextFactory));

            RuleFor(cmd => cmd.TicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));

            RuleFor(cmd => cmd.Title)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(UpdateTicketCommand.Title)));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>(nameof(UpdateTicketCommand.Priority)));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketType>(nameof(UpdateTicketCommand.TicketType)));

            RuleFor(cmd => cmd.TicketStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(UpdateTicketCommand.TicketStatus)));

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(UpdateTicketCommand.User)));

            // TODO: Add the same validation to AddTicketLinkCommand
            // TODO: Provide feedback which link is the offending one.
            RuleFor(cmd => cmd.Links)
                .Must((command, links) => !links.Any(link => link.TargetTicketId == command.TicketId))
                .WithMessage("A ticket link cannot be established to the same ticket.");

            RuleForEach(cmd => cmd.Links)
                .Must((command, link, context) =>
                {
                    var foundTicketIds = context.ParentContext.RootContextData[FoundTicketIdsContextDataKey] as ISet<int>;

                    return foundTicketIds.Contains(link.TargetTicketId);
                })
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("target ticket"));

            RuleForEach(cmd => cmd.Tags)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("tag"));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<UpdateTicketCommand> context, CancellationToken cancellation = default)
        {
            var targetTicketIds = context.InstanceToValidate.Links.Select(link => link.TargetTicketId).ToList();
            if (targetTicketIds.Count > 0)
            {
                using (var dbcontext = eventsContextFactory.CreateContext())
                {
                    context.RootContextData[FoundTicketIdsContextDataKey] = await dbcontext
                        .TicketCreatedEvents.Where(evt => targetTicketIds.Contains(evt.Id))
                        .Select(evt => evt.Id)
                        .ToSetAsync();
                }
            }
            else
            {
                context.RootContextData[FoundTicketIdsContextDataKey] = new HashSet<int>();
            }

            return await base.ValidateAsync(context, cancellation);
        }
    }
}