namespace UtilClasses.Db
{
    public interface IDbCall<out T> : IDbCall
    {
        T NullValue { get; }
        bool ReturnsEnumerable { get; set; }
    }

    public interface IDbCall
    {
        string Procedure { get; }
        ParameterHelper ParameterHelper { get; }
    }

    public interface IJsonDbCall : IDbCall
    {
    }

    public interface IJsonDbCall<T> : IJsonDbCall, IDbCall<T>
    {
    }

    public interface IValDbCall<T> : IDbCall where T:struct
    {
        
    }
}
