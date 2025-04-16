using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;

namespace UtilClasses.Web
{
    public class CommonExceptionFilter : IExceptionFilter
    {
        public bool AllowMultiple => false;

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext context, CancellationToken ct)
        {
            var ex = context.Exception;
            while (ex.InnerException != null)
                ex = ex.InnerException;
            context.Response = GetResponse(ex, context.Request);
            return Task.FromResult(0);
        }

        private HttpResponseMessage GetResponse(Exception exception, HttpRequestMessage request)
        {
            switch (exception)
            {
                case UnauthorizedAccessException ex:
                    return request.CreateErrorResponse(HttpStatusCode.Unauthorized,ex.Message);
                default:
                    return request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
            }
        }
    }
}
