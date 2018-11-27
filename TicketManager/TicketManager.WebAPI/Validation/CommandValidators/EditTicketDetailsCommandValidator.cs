using FluentValidation;
using Raven.Client.Documents;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class EditTicketDetailsCommandValidator : TicketCommandValidatorBase<EditTicketDetailsCommand>
    {
        public EditTicketDetailsCommandValidator(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(cmd => cmd.Editor)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(EditTicketDetailsCommand.Editor)));

            RuleFor(cmd => cmd.Title)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(EditTicketDetailsCommand.Title)));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>(nameof(EditTicketDetailsCommand.Priority)));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketType>(nameof(EditTicketDetailsCommand.TicketType)));

            RuleFor(cmd => cmd.TicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket(nameof(EditTicketDetailsCommand.TicketId)));
        }
    }
}