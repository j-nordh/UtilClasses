namespace UtilClasses.Interfaces
{
    public interface IMatchable<in T>
    {
        int GetMatchHash();
        bool Matches(T other);
    }
}
