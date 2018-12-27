using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.WebAPI.DTOs.Queries.Abstractions
{
    public class EnumChange<T> : Change<T> where T : Enum
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public override T ChangedTo { get; set; }
    }
}