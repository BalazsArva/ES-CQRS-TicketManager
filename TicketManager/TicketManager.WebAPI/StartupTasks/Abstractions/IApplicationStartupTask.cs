using System.Threading;
using System.Threading.Tasks;

namespace TicketManager.WebAPI.StartupTasks.Abstractions
{
    public interface IApplicationStartupTask
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}