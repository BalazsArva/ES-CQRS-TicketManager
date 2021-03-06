﻿using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Statistics
{
    public class TicketStatistics_CountByPriority : AbstractIndexCreationTask<Ticket, TicketStatistics_CountByPriority.IndexEntry>
    {
        public class IndexEntry
        {
            public TicketPriorities Priority { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_CountByPriority()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets =>
                from t in tickets
                select new IndexEntry
                {
                    Priority = t.TicketPriority.Priority,
                    Count = 1
                };

            Reduce = results =>
                from result in results
                group result by result.Priority into g
                select new IndexEntry
                {
                    Priority = g.Key,
                    Count = g.Sum(x => x.Count)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}