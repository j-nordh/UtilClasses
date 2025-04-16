using System.Threading;
using System.Threading.Tasks;

namespace UtilClasses.Interfaces
{
    public interface IPausable
    {
        Task Pause(CancellationToken ct);
        bool Paused { get; }
        void Resume();
    }
}
