using System;

namespace TicketManager.Common.Utils
{
    public static class StringSearchHelper
    {
        public static (StringSearchKind kind, string transformedValue) ParseSearchSyntax(string value)
        {
            // Syntax:
            // - ^abc -> search for values starting with abc
            // - abc$ -> search for values ending with abc
            // - ^abc$ -> search for values equal to abc
            // - \^abc -> treat ^ as actual input, not control character
            // - abc\$ -> treat $ as actual input, not control character
            // ~ should only be used to escape ^ at the start and $ at the end, not in any other locations in the string.
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.StartsWith("^") && value.EndsWith("$") && !value.EndsWith("\\$"))
            {
                return (StringSearchKind.Equals, value.Substring(1, value.Length - 2));
            }

            if (value.StartsWith("^"))
            {
                return (StringSearchKind.StartsWith, value.Substring(1));
            }

            if (value.EndsWith("$") && !value.EndsWith("\\$"))
            {
                return (StringSearchKind.EndsWith, value.Substring(0, value.Length - 1));
            }

            var transformedValue = value;
            if (transformedValue.StartsWith("\\^"))
            {
                transformedValue = transformedValue.Substring(1);
            }

            if (transformedValue.EndsWith("\\$"))
            {
                transformedValue = transformedValue.Substring(0, transformedValue.Length - 2) + '$';
            }

            return (StringSearchKind.SearchWithin, transformedValue);
        }
    }
}