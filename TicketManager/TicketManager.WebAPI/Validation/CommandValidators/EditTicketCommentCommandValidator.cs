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

        protected override ISet<int> ExtractReferencedCommentIds(ValidationContext<EditTicketCommentCommand> context)
        {
            return new HashSet<int>
            {
                context.InstanceToValidate.CommentId
            };
        }
    }
}