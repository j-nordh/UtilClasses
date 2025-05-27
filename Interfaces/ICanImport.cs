namespace UtilClasses.Interfaces;

public interface ICanImport<in T>
{
    void Import(T obj);
}