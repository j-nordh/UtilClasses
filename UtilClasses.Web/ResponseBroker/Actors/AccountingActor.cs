using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Tasks;

namespace UtilClasses.Web.ResponseBroker.Actors
{
    public class AccountingActor:IResponseActor
    {
        private static Dictionary<HttpStatusCode, int> _dict = new Dictionary<HttpStatusCode, int>();
        public void OnBadRequest() => Increase(HttpStatusCode.BadRequest);
        public void OnBadRequest(string message) => Increase(HttpStatusCode.BadRequest);
        public void OnBadRequest(Exception ex) => Increase(HttpStatusCode.BadRequest);
        public void OnBadRequest(ModelStateDictionary modelState) => Increase(HttpStatusCode.BadRequest);
        public void OnOk() => Increase(HttpStatusCode.OK);
        public void OnOk<T>(T content) => Increase(HttpStatusCode.OK);
        public void OnInternalServerError(Exception exception) => Increase(HttpStatusCode.InternalServerError);
        public void OnNotFound() => Increase(HttpStatusCode.NotFound);
        public void OnCreated<T>(Uri location, T content) => Increase(HttpStatusCode.Created);
        public void OnContent<T>(HttpStatusCode statusCode, T value) => Increase(HttpStatusCode.Created);
        public void OnStatusCode(HttpStatusCode status) => Increase(status);
        public void OnUnauthorized(string message) => Increase(HttpStatusCode.Unauthorized);
        public void OnResponseMessage(HttpResponseMessage response) => Increase(response.StatusCode);
        public void Logout() {}

        private void Increase(HttpStatusCode status)
        {
            Task.Run(() =>
            {
                lock (_dict)
                {
                    _dict.Increment(status);
                }
            }).Forget();
        }

        public static List<KeyValuePair<HttpStatusCode, int>> GetCounters()
        {
            lock (_dict)
            {
                return _dict.SnapshotList();
            }
        }
    }
}
