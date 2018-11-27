using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public UpdateTicketCommandValidator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new System.ArgumentNullException(nameof(eventsContextFactory));

            // TODO: Validate that the ticket exists
            /*
            RuleFor(cmd => cmd.TicketId)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(UpdateTicketCommand.TicketId)));
            */

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

            RuleFor(cmd => cmd.Links)
                .MustAsync(AllTicketsExist)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(UpdateTicketCommand.Links)));

            // TODO: Implement a per-tag feedback.
            RuleFor(cmd => cmd.Tags)
                .Must(tags => tags.All(tag => !string.IsNullOrWhiteSpace(tag)))
                .WithMessage("A tag cannot be empty or whitespace-only.");
        }

        private async Task<bool> AllTicketsExist(UpdateTicketCommand.TicketLink[] links, CancellationToken cancellationToken)
        {
            var targetTicketIds = links.Select(link => link.TargetTicketId).ToList();

            if (targetTicketIds.Count == 0)
            {
                return true;
            }

            using (var context = eventsContextFactory.CreateContext())
            {
                var foundIds = await context
                    .TicketCreatedEvents.Where(evt => targetTicketIds.Contains(evt.Id))
                    .Select(evt => evt.Id)
                    .ToListAsync();

                // TODO: Provide which were not found.
                return foundIds.Count == targetTicketIds.Count;
            }
        }
    }
}