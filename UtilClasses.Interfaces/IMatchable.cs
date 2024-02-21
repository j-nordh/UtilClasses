namespace UtilClasses.Interfaces
{
    public interface IMatchable<T>
    {
        int GetMatchHash();
        bool Matches(T obj);
    }
}
