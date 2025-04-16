using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using UtilClasses.Extensions.Exceptions;
using UtilClasses.Web.Properties;

namespace UtilClasses.Web.JWT
{
    public abstract class JwtLicenseRequiredAttribute:AuthorizationFilterAttribute
    {
        private readonly string _module;
        protected event EventHandler<Exception> ExceptionOccured;
        protected JwtLicenseRequiredAttribute(string module)
        {
            _module = module;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var validator = JwtLicenseValidator.Get;
            if (validator.IsLicensed(_module)) return;

            if(validator.Ex!= null) ExceptionOccured?.Invoke(this, validator.Ex);

            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.PaymentRequired);
            actionContext.Response.Content = new StringContent(Resources.LicenseMissing
                    .Replace("%module%", _module)
                    .Replace("%exception%", validator.Ex?.DeepToString() ?? ""),
                Encoding.UTF8, MediaTypeNames.Text.Html);
        }

        public static Exception LastException() => JwtLicenseValidator.Get.Ex;
        public static async Task<List<string>> Refresh()
        {
            var validator = JwtLicenseValidator.Get;
            await validator.RefreshAsync();
            return validator.Modules.ToList();
        }
    }
}
