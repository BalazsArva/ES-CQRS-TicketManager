﻿using FluentValidation;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class CreateTicketCommandValidator : ValidatorBase<CreateTicketCommand>
    {
        public CreateTicketCommandValidator()
        {
            RuleFor(cmd => cmd.Creator)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("creator"));

            RuleFor(cmd => cmd.Title)
                .Must(NotBeWhitespaceOnly)
                .WithMessage(ValidationMessageProvider.CannotBeNullOrEmptyOrWhitespace("title"));

            RuleFor(cmd => cmd.Priority)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<Priority>("priority"));

            RuleFor(cmd => cmd.TicketType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<TicketType>("ticket type"));
        }
    }
}