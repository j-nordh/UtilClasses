using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace UtilClasses.Web.JWT
{
    //public class JwtRequireAttribute : Attribute
    //{
    //    public Permissions Permission { get; }

    //    public JwtRequireAttribute(Permissions p)
    //    {
    //        Permission = p;
    //    }
    //}
    //public class JwtAuthorizeAttribute : AuthorizationFilterAttribute
    //{
    //    public static Authorizer Auth { get; set; }
    //    protected Authorizer.AuthResult _res;

    //    public override void OnAuthorization(HttpActionContext actionContext)
    //    {
    //        if (Thread.CurrentPrincipal.Identity.IsAuthenticated 
    //            || actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
    //            || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
    //        {
    //            return;
    //        }


    //        _res = Auth.Authorize(actionContext.Request.Headers.Authorization?.ToString());
    //        if (_res.Identity != null)
    //        {
    //            Thread.CurrentPrincipal = new GenericPrincipal(_res.Identity, null);
    //            actionContext.Request.Headers.Add("UserName", _res.Identity.Name);
    //        }
    //        else
    //            HandleUnauthorizedRequest(actionContext, _res.Reason ?? Authorizer.UnauthorizedReason.Forbidden);
    //    }



    //    protected void HandleUnauthorizedRequest(HttpActionContext actionContext, Authorizer.UnauthorizedReason reason)
    //    {
    //        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
    //        actionContext.Response.Content = new StringContent(reason.Description(), Encoding.UTF8, MediaTypeNames.Text.Plain);
    //    }
    //}
}