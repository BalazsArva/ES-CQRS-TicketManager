using System;

namespace TicketManager.Common.Utils
{
    public static class Throw
    {
        public static void IfNull<T>(string parameterName, T parameterValue)
            where T : class
        {
            if (parameterValue is null)
            {
                throw new ArgumentNullException(parameterName, $"The value of the '{parameterName}' cannot be null.");
            }
        }

        public static void IfNullOrWhiteSpace(string parameterName, string parameterValue)
        {
            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                throw new ArgumentException($"The value of the '{parameterName}' cannot be null, empty or whitespace-only.", parameterName);
            }
        }
    }
}