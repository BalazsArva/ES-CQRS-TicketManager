using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TicketManager.DataAccess.EntityFramework.Extensions
{
    public static class IQueryableExtensions
    {
        public static async Task<HashSet<T>> ToSetAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            var resultsList = await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);

            return new HashSet<T>(resultsList);
        }

        public static IQueryable<T> FromPage<T>(this IQueryable<T> queryable, int page, int pageSize)
        {
            return queryable.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}