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
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("commenter"));

            RuleFor(cmd => cmd.CommentText)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("comment text"));

            RuleFor(cmd => cmd.TicketId)
                .Must(BeAnExistingTicket)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket("ticket"));
        }

        protected override ISet<int> ExtractReferencedTicketIds(PostCommentToTicketCommand command)
        {
            return new HashSet<int> { command.TicketId };
        }
    }
}