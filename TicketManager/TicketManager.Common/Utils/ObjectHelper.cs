using System;
using System.Collections;

namespace TicketManager.Common.Utils
{
    public static class ObjectHelper
    {
        public static bool IsNumeric(object value)
        {
            return
                value is sbyte ||
                value is byte ||
                value is short ||
                value is ushort ||
                value is int ||
                value is uint ||
                value is long ||
                value is ulong ||
                value is float ||
                value is double ||
                value is decimal;
        }

        public static bool IsLogical(object value)
        {
            return value is bool;
        }

        public static bool IsStringLike(object value)
        {
            return value is string || value is char;
        }

        public static bool IsDateTimeLike(object value)
        {
            return value is DateTime || value is DateTimeOffset;
        }

        public static bool IsEnum(object value)
        {
            return value is Enum;
        }

        public static bool IsPrimitive(object value)
        {
            return
                IsNumeric(value) ||
                IsLogical(value) ||
                IsStringLike(value) ||
                IsEnum(value) ||
                value == null;
        }

        public static bool IsCollection(object value)
        {
            return value is ICollection;
        }
    }
}