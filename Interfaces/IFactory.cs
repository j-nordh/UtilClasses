using System.Threading.Tasks;

namespace UtilClasses.Interfaces
{
    public interface IFactory<out TOut, in TCfg>
    {
        TOut Build(TCfg cfg);
    }
    public interface IAsyncFactory<TOut, in TCfg>
    {
        Task<TOut> Build(TCfg cfg);
    }
}
