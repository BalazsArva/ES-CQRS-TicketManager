using System;
using Raven.Client.Documents.Linq;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class IRavenQueryableExtensions
    {
        public static IRavenQueryable<T> Paginate<T>(this IRavenQueryable<T> queryable, int page, int pageSize)
        {
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "The requested page cannot be less than 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "The number of requested items cannot be less than 1.");
            }

            return queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
    }
}