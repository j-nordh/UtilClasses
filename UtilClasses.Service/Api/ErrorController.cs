using System;
using System.Net;
using JetBrains.Annotations;
using MACS.Dto;
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

        return new ObjectResult(GetException(exceptionHandlerFeature.Error))
            {StatusCode = (int) HttpStatusCode.InternalServerError};
    }

    private ExceptionWrapper GetException(Exception e) => null == e
        ? null
        : new()
        {
            Title = e.Message,
            StackTrace = e.StackTrace,
            InnerException = GetException(e.InnerException)
        };
}