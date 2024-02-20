using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using log4net;
using UtilClasses.Logging;

namespace UtilClasses.Web.Logging
{
    public class Log4NetExceptionLogger :ExceptionLogger
    {
        private readonly ILog _log;

        public Log4NetExceptionLogger(ILog log)
        {
            _log = log;
        }

        public override void Log(ExceptionLoggerContext context)
        {

            var controller = context.ExceptionContext.ControllerContext?.Controller as ApiController;
            var ps = context.RequestContext.Url.Request.GetQueryNameValuePairs();
            var lp = LogParams.Create(ps, controller?.GetType().Name ?? "No controller", controller?.User.Identity.Name);
            _log.Error(lp.GetErrorString(context.Exception, true));
        }
    }
}
