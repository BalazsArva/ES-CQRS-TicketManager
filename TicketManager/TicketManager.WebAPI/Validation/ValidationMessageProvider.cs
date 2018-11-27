using System;

namespace TicketManager.WebAPI.Validation
{
    public static class ValidationMessageProvider
    {
        public static string CannotBeNull(string propertyName) => $"'{propertyName}' cannot be null.";

        public static string CannotBeNullOrEmpty(string propertyName) => $"'{propertyName}' cannot be null or empty.";

        public static string CannotBeNullOrEmptyOrWhitespace(string propertyName) => $"'{propertyName}' cannot be null, empty or whitespace-only.";

        public static string OnlyTheseValuesAreAllowed<TValue>(string propertyName, params TValue[] allowedValues) =>
            $"Only the following values are accepted for '{propertyName}': {string.Join(", ", allowedValues)}";

        public static string OnlyEnumValuesAreAllowed<TEnum>(string propertyName) where TEnum : Enum
        {
            var allowedValues = Enum.GetNames(typeof(TEnum));

            return OnlyTheseValuesAreAllowed(propertyName, allowedValues);
        }

        public static string MustReferenceAnExistingTicket(string propertyName) => $"'{propertyName}' must be an existent ticket.";
    }
}