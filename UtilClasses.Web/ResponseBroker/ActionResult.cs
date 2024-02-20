using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace UtilClasses.Web.ResponseBroker
{
    public class ActionResult<T>: IHttpActionResult
    {
        private readonly IHttpActionResult _res;
        public ActionResult(IHttpActionResult res)
        {
            _res = res;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) => await _res.ExecuteAsync(cancellationToken);
    }
}
