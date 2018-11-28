using FluentValidation;
using Raven.Client.Documents;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class EditTicketCommentCommandValidator : TicketCommandValidatorBase<EditTicketCommentCommand>
    {
        public EditTicketCommentCommandValidator(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("editor"));

            RuleFor(cmd => cmd.CommentText)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty("comment text"));

            RuleFor(cmd => cmd.CommentId)
                .MustAsync(CommentExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingComment("modified comment"));
        }
    }
}