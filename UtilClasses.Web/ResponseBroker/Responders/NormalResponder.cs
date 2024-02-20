using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;

namespace UtilClasses.Web.ResponseBroker.Responders
{
    public class NormalResponder:IResponder
    {
        private RequestProvider _provider;

        public NormalResponder(HttpRequestMessage request)
        {
            _provider = new RequestProvider(request);
        }

        public NormalResponder(ApiController controller)
        {
            _provider = new RequestProvider(controller);
        }

        public virtual IResult<T> BadRequest<T>() 
            => Create<T>(HttpStatusCode.BadRequest);

        public virtual IResult<T> BadRequest<T>(Exception ex) 
            => Create<T>(HttpStatusCode.BadRequest, ex);

        public virtual IHttpActionResult BadRequest(ModelStateDictionary modelState)
            => Create(HttpStatusCode.BadRequest, modelState);

        public virtual IHttpActionResult Ok() 
            => Create<HttpStatusCode>(HttpStatusCode.OK);

        public virtual IResult<T> Ok<T>(T content) 
            => Create(HttpStatusCode.OK, content);

        public IResult<T> ShapedOk<T>(object content)
        {
            return Create<T>(HttpStatusCode.OK, content);
        }

        public virtual IResult<T> InternalServerError<T>(Exception exception)
            => Create<T>(HttpStatusCode.InternalServerError, exception);
        public virtual IHttpActionResult InternalServerError(Exception exception)
            => Create(HttpStatusCode.InternalServerError, exception);

        public virtual IHttpActionResult NotFound() 
            => Create<HttpStatusCode>(HttpStatusCode.NotFound);

        public virtual IResult<T> Created<T>(Uri location, T content) 
            => Create(HttpStatusCode.Created, content);

        public virtual IHttpActionResult Content<T>(HttpStatusCode statusCode, T value) 
            => Create(statusCode, value);

        public virtual IResult<T> StatusCode<T>(HttpStatusCode status) 
            => Create<T>(status);

        public virtual IResult<T> Unauthorized<T>(string message)
            => Create<T>(HttpStatusCode.Unauthorized, message);

        public virtual IResult<T> ResponseMessage<T>(HttpResponseMessage response)
            => new Result<T>(new ResponseMessageResult(response));

        public IResult<T> NotFound<T>()
            => Create<T>(HttpStatusCode.NotFound);

        IResult<T> Create<T>(HttpStatusCode code) => ResponseMessage<T>(_provider.Request.CreateResponse(code));

        private IResult<T> Create<T>(HttpStatusCode code, T obj)
            => ResponseMessage<T>(_provider.Request.CreateResponse(code, obj));

        private IResult<T> Create<T>(HttpStatusCode code, object obj)
            => ResponseMessage<T>(_provider.Request.CreateResponse(code, obj));

        private IResult<T> Create<T>(HttpStatusCode code, string message)
            => ResponseMessage<T>(_provider.Request.CreateResponse(code, message));
        
        private IResult<T> Create<T>(HttpStatusCode code, Exception ex)
            => ResponseMessage<T>(_provider.Request.CreateResponse(code, ex));
    }
}
