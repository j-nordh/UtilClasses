namespace UtilClasses.Db
{
    public interface IKeywordStore
    {
        int this[string keyword] { get; }
        string this[int i] { get; }

        int Get(string keyword);
    }
}