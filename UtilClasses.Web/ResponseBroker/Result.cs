using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace UtilClasses.Web.ResponseBroker
{
    public class Result<T>:IResult<T>
    {
        private readonly IHttpActionResult _har;

        public Result(IHttpActionResult har)
        {
            _har = har;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) =>
            _har.ExecuteAsync(cancellationToken);
    }
}