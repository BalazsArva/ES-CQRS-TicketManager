using Microsoft.EntityFrameworkCore;

namespace TicketManager.DataAccess.EntityFramework
{
    public interface IDbContextFactory<TDbContext>
        where TDbContext : DbContext
    {
        TDbContext CreateContext();
    }
}