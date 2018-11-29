using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class PostCommentToTicketCommandValidator : TicketCommandValidatorBase<PostCommentToTicketCommand>
    {
        public PostCommentToTicketCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("commenter"));

            RuleFor(cmd => cmd.CommentText)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("comment text"));

            RuleFor(cmd => cmd.TicketId)
                .Must(TicketExists)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(ValidationContext<PostCommentToTicketCommand> context)
        {
            return new HashSet<int>
            {
                context.InstanceToValidate.TicketId
            };
        }
    }
}