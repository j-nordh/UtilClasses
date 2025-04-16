using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.ModelBinding;

namespace UtilClasses.Web.ResponseBroker.Actors
{
    public interface IResponseActor
    {
        void OnBadRequest();
        void OnBadRequest(string message);
        void OnBadRequest(Exception ex);
        void OnBadRequest(ModelStateDictionary modelState);
        void OnOk();
        void OnOk<T>(T content);
        void OnInternalServerError(Exception exception);
        void OnNotFound();
        void OnCreated<T>(Uri location, T content);
        void OnContent<T>(HttpStatusCode statusCode, T value);
        void OnStatusCode(HttpStatusCode status);
        void OnUnauthorized(string message);
        void OnResponseMessage(HttpResponseMessage response);
        void Logout();
    }
}
