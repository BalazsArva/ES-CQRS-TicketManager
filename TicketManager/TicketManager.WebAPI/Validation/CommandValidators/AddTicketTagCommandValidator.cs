using FluentValidation;
using Raven.Client.Documents;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AddTicketTagCommandValidator : TicketCommandValidatorBase<AddTicketTagCommand>
    {
        public AddTicketTagCommandValidator(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AddTicketTagCommand.User)));

            RuleFor(cmd => cmd.Tag)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AddTicketTagCommand.Tag)));

            RuleFor(cmd => cmd.TicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket(nameof(AddTicketTagCommand.TicketId)));
        }
    }
}