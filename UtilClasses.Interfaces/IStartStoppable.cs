namespace UtilClasses.Interfaces
{
    public interface IStartStoppable
    {
        void Start();
        void Stop(bool wait);
    }
}
