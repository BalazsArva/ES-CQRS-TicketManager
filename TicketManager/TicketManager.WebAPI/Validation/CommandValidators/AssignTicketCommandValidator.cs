using FluentValidation;
using Raven.Client.Documents;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AssignTicketCommandValidator : TicketCommandValidatorBase<AssignTicketCommand>
    {
        public AssignTicketCommandValidator(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(cmd => cmd.Assigner)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AssignTicketCommand.Assigner)));

            RuleFor(cmd => cmd.TicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket(nameof(AssignTicketCommand.TicketId)));
        }
    }
}