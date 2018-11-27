using FluentValidation;
using Raven.Client.Documents;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class PostCommentToTicketCommandValidator : TicketCommandValidatorBase<PostCommentToTicketCommand>
    {
        public PostCommentToTicketCommandValidator(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(PostCommentToTicketCommand.User)));

            RuleFor(cmd => cmd.CommentText)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(PostCommentToTicketCommand.CommentText)));

            RuleFor(cmd => cmd.TicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket(nameof(PostCommentToTicketCommand.TicketId)));
        }
    }
}