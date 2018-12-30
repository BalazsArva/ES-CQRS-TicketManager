using System;
using System.Globalization;

namespace TicketManager.WebAPI.Conventions
{
    public static class DateTimeConventions
    {
        public static DateTime GetUniversalDateTime(string value)
        {
            return DateTime.Parse(value, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal).ToUniversalTime();
        }

        public static bool IsValid(string value)
        {
            return DateTime.TryParse(value, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out var _);
        }
    }
}