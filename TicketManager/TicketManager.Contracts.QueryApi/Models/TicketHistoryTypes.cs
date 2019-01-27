using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketManager.Contracts.QueryApi.Models
{
    public static class TicketHistoryTypes
    {
        public const char HistoryTypeSeparator = ',';

        public const string Title = "title";
        public const string Description = "description";
        public const string Status = "status";
        public const string StoryPoints = "storypoints";
        public const string Type = "type";
        public const string Priority = "priority";
        public const string Assignment = "assignment";
        public const string Tags = "tags";
        public const string Links = "links";

        public static IEnumerable<string> ValidValues { get; } = new HashSet<string>(new[]
        {
            Title,
            Description,
            Status,
            StoryPoints,
            Type,
            Priority,
            Assignment,
            Tags,
            Links
        });

        public static IEnumerable<string> GetRequestedHistoryTypes(string requestedTypes)
        {
            if (string.IsNullOrEmpty(requestedTypes))
            {
                return Array.Empty<string>();
            }

            return new HashSet<string>(requestedTypes.Split(new[] { HistoryTypeSeparator }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()));
        }
    }
}