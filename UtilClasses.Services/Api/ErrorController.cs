using System;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using UtilClasses;

namespace MACS.Service.Api;

[Controller]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : Controller
{
    [Route("/error")]
    public IActionResult HandleErrorDevelopment(
        [FromServices] IHostEnvironment hostEnvironment)
    {
        var exceptionHandlerFeature =
            HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        return new ObjectResult(ExceptionWrapper.From(exceptionHandlerFeature.Error))
            {StatusCode = (int) HttpStatusCode.InternalServerError};
    }
}