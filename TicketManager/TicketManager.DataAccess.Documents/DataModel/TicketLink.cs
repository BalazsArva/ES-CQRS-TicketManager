﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketLink : IEquatable<TicketLink>
    {
        public string TargetTicketId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketLinkTypes LinkType { get; set; }

        public bool Equals(TicketLink other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == obj as TicketLink;
        }

        public override int GetHashCode()
        {
            return
                (TargetTicketId.GetHashCode() * 3) ^
                (LinkType.GetHashCode() * 5);
        }

        public static bool operator ==(TicketLink lhs, TicketLink rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if ((object)lhs == null || (object)rhs == null)
            {
                return false;
            }

            return
                lhs.TargetTicketId == rhs.TargetTicketId &&
                lhs.LinkType == rhs.LinkType;
        }

        public static bool operator !=(TicketLink lhs, TicketLink rhs)
        {
            return !(lhs == rhs);
        }
    }
}