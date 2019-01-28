﻿using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.TicketStatistics
{
    public class TicketStatistics_StatusCountsByUser : AbstractIndexCreationTask<Ticket, TicketStatistics_StatusCountsByUser.IndexEntry>
    {
        public class IndexEntry
        {
            public string User { get; set; }

            public TicketStatuses Status { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_StatusCountsByUser()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets => 
                from t in tickets
                select new IndexEntry
                {
                    User = t.Assignment.AssignedTo,
                    Status = t.TicketStatus.Status,
                    Count = 1
                };

            Reduce = results => 
                from result in results
                group result by new { result.User, result.Status } into g
                select new IndexEntry
                {
                    User = g.Key.User,
                    Status = g.Key.Status,
                    Count = g.Sum(x => x.Count)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}