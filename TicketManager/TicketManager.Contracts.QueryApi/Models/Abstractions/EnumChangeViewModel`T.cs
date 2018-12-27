using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.Contracts.QueryApi.Models.Abstractions
{
    public class EnumChangeViewModel<T> : ChangeViewModel<T> where T : Enum
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public override T ChangedTo { get; set; }
    }
}