using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace UtilClasses.Web.ResponseBroker.Responders
{
    public interface IResponder
    {
        IResult<T> BadRequest<T>();
        IResult<T> BadRequest<T>(Exception ex);
        IHttpActionResult Ok();
        IResult<T> Ok<T>(T content);
        IResult<T> ShapedOk<T>(object content);
        IResult<T> InternalServerError<T>(Exception exception);
        IHttpActionResult InternalServerError(Exception exception);
        IHttpActionResult NotFound();
        IResult<T> Created<T>(Uri location, T content);
        IHttpActionResult Content<T>(HttpStatusCode statusCode, T value);
        IResult<T> StatusCode<T>(HttpStatusCode status);
        IResult<T> Unauthorized<T>(string message);
        IResult<T> ResponseMessage<T>(HttpResponseMessage response);
        IResult<T> NotFound<T>();
    }
}
