﻿using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Statistics
{
    public class TicketStatistics_CountByType : AbstractIndexCreationTask<Ticket, TicketStatistics_CountByType.IndexEntry>
    {
        public class IndexEntry
        {
            public TicketTypes Type { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_CountByType()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets =>
                from t in tickets
                select new IndexEntry
                {
                    Type = t.TicketType.Type,
                    Count = 1
                };

            Reduce = results =>
                from result in results
                group result by result.Type into g
                select new IndexEntry
                {
                    Type = g.Key,
                    Count = g.Sum(x => x.Count)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}