﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketDetails
    {
        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}