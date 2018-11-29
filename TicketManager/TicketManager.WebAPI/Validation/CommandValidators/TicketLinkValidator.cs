using System;
using System.Collections.Generic;
using FluentValidation;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs;

namespace TicketManager.WebAPI.Validation.CommandValidators
{
    public class TicketLinkValidator : AbstractValidator<TicketLinkDTO>
    {
        public TicketLinkValidator()
        {
            RuleFor(link => link.LinkType)
                .IsInEnum()
                .WithMessage(ValidationMessageProvider.OnlyEnumValuesAreAllowed<LinkType>("link type"));

            RuleFor(link => link.TargetTicketId)
                .Must((link, index, context) =>
                {
                    if (context.ParentContext.RootContextData[ValidationContextKeys.FoundTicketIdsContextDataKey] is ISet<int> foundTicketIds)
                    {
                        return foundTicketIds.Contains(link.TargetTicketId);
                    }

                    throw new InvalidOperationException(
                        "The validation could not be performed because the collection of existing ticket identifiers was not found in the context data of the validation context.");
                });
        }
    }
}