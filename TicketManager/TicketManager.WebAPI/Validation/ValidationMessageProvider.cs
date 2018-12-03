using System;

namespace TicketManager.WebAPI.Validation
{
    public static class ValidationMessageProvider
    {
        public static string CannotBeNull(string propertyDisplayName) => $"The {propertyDisplayName} cannot be null.";

        public static string CannotBeNullOrEmptyCollection(string elementDisplayName) => $"At least one {elementDisplayName} must be provided.";

        // Do not include null in the message because that's technical, the general user won't understand it.
        public static string CannotBeNullOrEmpty(string propertyDisplayName) => $"The {propertyDisplayName} cannot be empty.";

        // Do not include null in the message because that's technical, the general user won't understand it.
        public static string CannotBeNullOrEmptyOrWhitespace(string propertyDisplayName) => $"The {propertyDisplayName} cannot be empty or whitespace-only.";

        public static string OnlyTheseValuesAreAllowed<TValue>(string propertyDisplayName, params TValue[] allowedValues) =>
            $"Only the following values are accepted for {propertyDisplayName}: {string.Join(", ", allowedValues)}";

        public static string OnlyEnumValuesAreAllowed<TEnum>(string propertyDisplayName) where TEnum : Enum
        {
            var allowedValues = Enum.GetNames(typeof(TEnum));

            return OnlyTheseValuesAreAllowed(propertyDisplayName, allowedValues);
        }

        public static string MustReferenceAnExistingTicket(string propertyDisplayName) => $"The {propertyDisplayName} must be an existing ticket.";

        public static string MustReferenceAnExistingComment(string propertyDisplayName) => $"The {propertyDisplayName} must be an existing comment.";

        public static string MustNotBeAnAssignedTag(string propertyDisplayName) => $"The {propertyDisplayName} is already assigned to the ticket.";

        public static string MustBeAnAssignedTag(string propertyDisplayName) => $"The {propertyDisplayName} is not assigned to the ticket.";

        public static string MustNotBeAnAssignedLink() => "This ticket is already linked.";
    }
}