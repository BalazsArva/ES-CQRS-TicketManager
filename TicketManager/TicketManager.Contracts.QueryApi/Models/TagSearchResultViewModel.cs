﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TagSearchResultViewModel
    {
        public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

        public bool IsStale { get; set; }

        public DateTime IndexTimestamp { get; set; }
    }
}