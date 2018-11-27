using FluentValidation;
using Raven.Client.Documents;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class AddTicketLinkCommandValidator : TicketCommandValidatorBase<AddTicketLinkCommand>
    {
        public AddTicketLinkCommandValidator(IDocumentStore documentStore)
            : base(documentStore)
        {
            RuleFor(cmd => cmd.User)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmpty(nameof(AddTicketLinkCommand.User)));

            RuleFor(cmd => cmd.LinkType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<LinkType>(nameof(AddTicketLinkCommand.LinkType)));

            RuleFor(cmd => cmd.SourceTicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket(nameof(AddTicketLinkCommand.SourceTicketId)));

            RuleFor(cmd => cmd.TargetTicketId)
                .MustAsync(TicketExistsAsync)
                .WithMessage(ValidationMessageProvider.MustReferenceAnExistingTicket(nameof(AddTicketLinkCommand.TargetTicketId)));
        }
    }
}