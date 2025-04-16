using System.Threading.Tasks.Dataflow;

namespace UtilClasses.Dataflow;

public interface ISource<out T>
{
    public ISourceBlock<T> OutBlock { get; }
}