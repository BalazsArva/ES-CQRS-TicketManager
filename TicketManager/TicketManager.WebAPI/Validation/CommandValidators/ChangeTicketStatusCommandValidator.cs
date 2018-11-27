using FluentValidation;
using Raven.Client.Documents;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class ChangeTicketStatusCommandValidator : TicketCommandValidatorBase<ChangeTicketStatusCommand>
    {
        public ChangeTicketStatusCommandValidator(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(ChangeTicketStatusCommand.User)));

            RuleFor(cmd => cmd.NewStatus)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>(nameof(ChangeTicketStatusCommand.NewStatus)));

            RuleFor(cmd => cmd.TicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket(nameof(ChangeTicketStatusCommand.TicketId)));
        }
    }
}