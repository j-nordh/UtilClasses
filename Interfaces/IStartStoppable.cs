using System.Threading.Tasks;

namespace UtilClasses.Interfaces
{
    public interface IStartStoppable
    {
        void Start();
        void Stop(bool wait);
    }
    public interface IStartStoppableAsync
    {
        Task StartAsync();
        Task StopAsync();
    }
}
