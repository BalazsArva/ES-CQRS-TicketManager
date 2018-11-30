using System.Collections.Generic;
using FluentValidation;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class EditTicketCommentCommandValidator : CommentCommandValidatorBase<EditTicketCommentCommand>
    {
        public EditTicketCommentCommandValidator(IEventsContextFactory eventsContextFactory)
            : base(eventsContextFactory)
        {
            RuleFor(cmd => cmd.User)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("editor"));

            RuleFor(cmd => cmd.CommentText)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("comment text"));

            RuleFor(cmd => cmd.CommentId)
                .Must(BeAnExistingComment)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingComment("modified comment"));
        }

        protected override ISet<int> ExtractReferencedCommentIds(EditTicketCommentCommand command)
        {
            return new HashSet<int> { command.CommentId };
        }
    }
}