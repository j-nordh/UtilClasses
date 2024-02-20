using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Hogia.TW.API.Extensions;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Web.ResponseBroker.Actors;
using UtilClasses.Web.ResponseBroker.Responders;

namespace UtilClasses.Web.ResponseBroker
{
    public class Broker:IResponder
    {
        private readonly List<IResponseActor> _actors;
        private readonly IResponder _responder;

        public Broker(IResponder responder, params IResponseActor[] actors)
        {
            _actors = actors.ToList();
            _responder = responder;
        }

        public IResult<T> BadRequest<T>()
        {
            _actors.ForEach(a=>a.OnBadRequest());
            return _responder.BadRequest<T>();
        }

     

        public IResult<T> BadRequest<T>(Exception ex)
        {
            _actors.ForEach(a => a.OnBadRequest(ex));
            return _responder.BadRequest<T>(ex);
        }

        public IHttpActionResult Ok()
        {
            _actors.ForEach(a=>a.OnOk());
            return _responder.Ok();
        }

        public IHttpActionResult Ok(Action a)
        {
            try
            {
                a();
                return Ok();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        public async Task<IHttpActionResult> Ok(Task t)
        {
            try
            {
                await t;
                return Ok();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

        }

        public IResult<T> Ok<T>(T content)
        {
            _actors.ForEach(a=>a.OnOk(content));
            return _responder.Ok(content);
        }

        public IResult<T> ShapedOk<T>(object content)
        {
            _actors.ForEach(a=>a.OnOk(content));
            return _responder.ShapedOk<T>(content);
        }

        public IResult<List<T>> ShapeOk<T>(IEnumerable<T> content, string fields) =>
            ShapeOk(content, fields?.ToLower().Split(',').ToList());


        public IResult<List<T>> ShapeOk<T>(IEnumerable<T> content, List<string> lstOfFields) =>
            ShapedOk<List<T>>(content.Select(c => c.Shape(lstOfFields)).ToList());
        public IResult<T> ShapeOk<T>(T content, string fields) => ShapeOk(content, fields?.ToLower().Split(',').ToList());

        public IResult<T> ShapeOk<T>(T content, List<string> lstOfFields)
        {
            return lstOfFields.IsNullOrEmpty()? Ok(content): ShapedOk<T>(content.Shape(lstOfFields));
        }

        public IResult<T> InternalServerError<T>(Exception exception)
        {
            _actors.ForEach(a => a.OnInternalServerError(exception));
            return _responder.InternalServerError<T>(exception);
        }
        public IHttpActionResult InternalServerError(Exception exception)
        {
            _actors.ForEach(a => a.OnInternalServerError(exception));
            return _responder.InternalServerError(exception);
        }

        public IHttpActionResult NotFound()
        {
            _actors.ForEach(a => a.OnNotFound());
            return _responder.NotFound();
        }

        public IResult<T> NotFound<T>()
        {
            _actors.ForEach(a=>a.OnNotFound());
            return _responder.NotFound<T>();
        }

        public IResult<T> Created<T>(Uri location, T content)
        {
            _actors.ForEach(a => a.OnCreated(location,content));
            return _responder.Created(location, content);
        }

        public IHttpActionResult Content<T>(HttpStatusCode statusCode, T value)
        {
            _actors.ForEach(a => a.OnContent(statusCode, value));
            return _responder.Content(statusCode, value);
        }

        public IResult<T> StatusCode<T>(HttpStatusCode status)
        {
            _actors.ForEach(a => a.OnStatusCode(status));
            return _responder.StatusCode<T>(status);
        }

        public IResult<T> Unauthorized<T>(string message)
        {
            _actors.ForEach(a => a.OnUnauthorized(message));
            return _responder.Unauthorized<T>(message);
        }

        public IResult<T> ResponseMessage<T>(HttpResponseMessage response)
        {
            _actors.ForEach(a => a.OnResponseMessage(response));
            return _responder.ResponseMessage<T>(response);
        }

        public IHttpActionResult Logout(int i)
        {
            _actors.ForEach(a=>a.Logout());
            return _responder.Ok();
        }
    }
}
