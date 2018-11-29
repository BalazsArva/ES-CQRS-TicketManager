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
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("editor"));

            RuleFor(cmd => cmd.CommentText)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("comment text"));

            RuleFor(cmd => cmd.CommentId)
                .Must(CommentExists)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingComment("modified comment"));
        }

        protected override ISet<int> ExtractReferencedCommentIds(EditTicketCommentCommand command)
        {
            return new HashSet<int> { command.CommentId };
        }
    }
}