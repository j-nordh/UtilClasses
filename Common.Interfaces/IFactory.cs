using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IFactory<out TOut, in TCfg>
    {
        public TOut Build(TCfg cfg);
    }
    public interface IAsyncFactory<TOut, in TCfg>
    {
        Task<TOut> Build(TCfg cfg);
    }
}
