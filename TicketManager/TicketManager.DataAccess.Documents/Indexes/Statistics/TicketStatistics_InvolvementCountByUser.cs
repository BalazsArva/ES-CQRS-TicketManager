﻿using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Statistics
{
    public class TicketStatistics_InvolvementCountByUser : AbstractIndexCreationTask<Ticket, TicketStatistics_InvolvementCountByUser.IndexEntry>
    {
        public class IndexEntry
        {
            public string User { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_InvolvementCountByUser()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets => 
                from t in tickets
                from user in t.Involvement.InvolvedUsersSet
                select new IndexEntry
                {
                    User = user,
                    Count = 1
                };

            Reduce = results => 
                from result in results
                group result by result.User into g
                select new IndexEntry
                {
                    User = g.Key,
                    Count = g.Sum(x => x.Count)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}