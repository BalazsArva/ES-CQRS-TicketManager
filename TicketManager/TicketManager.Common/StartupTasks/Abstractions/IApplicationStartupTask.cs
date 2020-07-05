using System.Threading;
using System.Threading.Tasks;

namespace TicketManager.Common.StartupTasks.Abstractions
{
    public interface IApplicationStartupTask
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}