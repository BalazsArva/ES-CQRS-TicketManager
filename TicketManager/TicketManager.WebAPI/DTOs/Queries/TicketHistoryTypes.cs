using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public static class TicketHistoryTypes
    {
        public const char HistoryTypeSeparator = ',';

        public const string Title = "title";
        public const string Description = "description";
        public const string Status = "status";
        public const string Type = "type";
        public const string Priority = "priority";
        public const string Assignment = "assignment";

        public static IEnumerable<string> ValidValues { get; } = new HashSet<string>(new[]
        {
            Title,
            Description,
            Status,
            Type,
            Priority,
            Assignment
        });

        public static IEnumerable<string> GetRequestedHistoryTypes(string requestedTypes)
        {
            if (string.IsNullOrEmpty(requestedTypes))
            {
                return Array.Empty<string>();
            }

            return requestedTypes.Split(HistoryTypeSeparator, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()).ToHashSet();
        }
    }
}