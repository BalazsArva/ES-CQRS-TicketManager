using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;
using TicketManager.WebAPI.Validation.CommandValidators.Abstractions;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketStoryPointsCommandValidator : TicketCommandValidatorBase<ChangeTicketStoryPointsCommand>
    {
        public ChangeTicketStoryPointsCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.StoryPoints)
                .GreaterThanOrEqualTo(ValidationConstants.MinStoryPoints)
                .WithMessage(ValidationMessageProvider.CannotBeNegative("story points"));
        }
    }
}