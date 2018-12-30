using System;
using FluentValidation;
using TicketManager.WebAPI.Conventions;

namespace TicketManager.WebAPI.Validation
{
    public class ValidatorBase<T> : AbstractValidator<T>
    {
        protected bool NotBeWhitespaceOnly(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        protected bool BeValidCaseInsensitiveEnumString<TEnum>(string value)
            where TEnum : struct, Enum
        {
            return IsValidEnumString<TEnum>(value, true);
        }

        protected bool BeValidCaseSensitiveEnumString<TEnum>(string value)
            where TEnum : struct, Enum
        {
            return IsValidEnumString<TEnum>(value, false);
        }

        protected bool BeValidIsoDateString(string value)
        {
            return DateTimeConventions.IsValid(value);
        }

        protected bool IsValidEnumString<TEnum>(string value, bool ignoreCase)
            where TEnum : struct, Enum
        {
            return Enum.TryParse<TEnum>(value, ignoreCase, out var _);
        }
    }
}