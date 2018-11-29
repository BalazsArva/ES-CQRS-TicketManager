using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TicketManager.DataAccess.Events.Extensions
{
    public static class IQueryableExtensions
    {
        public static async Task<HashSet<T>> ToSetAsync<T>(this IQueryable<T> queryable)
        {
            var resultsList = await queryable.ToListAsync();

            return new HashSet<T>(resultsList);
        }
    }
}