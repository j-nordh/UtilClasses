using System.Threading.Tasks.Dataflow;

namespace UtilClasses.Dataflow;

public interface ITarget<in T>
{
    public ITargetBlock<T> InBlock { get; }
}