using System;
using FluentValidation;

namespace TicketManager.WebAPI.Validation
{
    public class ValidatorBase<T> : AbstractValidator<T>
    {
        protected bool NotBeWhitespaceOnly(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        protected bool BeValidEnumString<TEnum>(string value)
            where TEnum : struct, Enum
        {
            return Enum.TryParse<TEnum>(value, out var _);
        }
    }
}