using FluentValidation;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class PostCommentToTicketCommandValidator : AbstractValidator<PostCommentToTicketCommand>
    {
        public PostCommentToTicketCommandValidator()
        {
            // TODO: Use the query model (once it is implemented) to verify the existence of the ticket.

            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(PostCommentToTicketCommand.User)));

            RuleFor(cmd => cmd.CommentText)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(PostCommentToTicketCommand.CommentText)));
        }
    }
}