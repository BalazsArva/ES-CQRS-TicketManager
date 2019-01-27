using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketBasicDetailsViewModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string AssignedTo { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdatedBy { get; set; }

        public int StoryPoints { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public DateTime UtcDateLastUpdated { get; set; }

        public string[] InvolvedUsers { get; set; } = Array.Empty<string>();

        public string[] Tags { get; set; } = Array.Empty<string>();

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketStatuses Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketTypes Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketPriorities Priority { get; set; }
    }
}