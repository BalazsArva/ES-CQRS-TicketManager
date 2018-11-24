using FluentValidation;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class EditTicketCommentCommandValidator : AbstractValidator<EditTicketCommentCommand>
    {
        public EditTicketCommentCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the comment.

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(EditTicketCommentCommand.User)));

            RuleFor(cmd => cmd.CommentText)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(EditTicketCommentCommand.CommentText)));
        }
    }
}