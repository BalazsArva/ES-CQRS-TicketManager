using System;

namespace TicketManager.WebAPI.Validation
{
    public static class ValidationMessageProvider
    {
        public static string CannotBeNull(string propertyDisplayName) => $"The {propertyDisplayName} cannot be null.";

        public static string CannotBeNullOrEmpty(string propertyDisplayName) => $"The {propertyDisplayName} cannot be null or empty.";

        public static string CannotBeNullOrEmptyOrWhitespace(string propertyDisplayName) => $"The {propertyDisplayName} cannot be null, empty or whitespace-only.";

        public static string OnlyTheseValuesAreAllowed<TValue>(string propertyDisplayName, params TValue[] allowedValues) =>
            $"Only the following values are accepted for {propertyDisplayName}: {string.Join(", ", allowedValues)}";

        public static string OnlyEnumValuesAreAllowed<TEnum>(string propertyDisplayName) where TEnum : Enum
        {
            var allowedValues = Enum.GetNames(typeof(TEnum));

            return OnlyTheseValuesAreAllowed(propertyDisplayName, allowedValues);
        }

        public static string MustReferenceAnExistingTicket(string propertyDisplayName) => $"The {propertyDisplayName} must be an existing ticket.";

        public static string MustReferenceAnExistingComment(string propertyDisplayName) => $"The {propertyDisplayName} must be an existing comment.";
    }
}