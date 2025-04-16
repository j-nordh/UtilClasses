using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace UtilClasses.Web.ResponseBroker.Responders
{
    class RequestProvider
    {
        private readonly ApiController _controller;
        private readonly HttpRequestMessage _request;

        public RequestProvider(HttpRequestMessage request)
        {
            _request = request;
        }

        public RequestProvider(ApiController controller)
        {
            _controller = controller;
        }

        public HttpRequestMessage Request
        {
            get
            {
                if (null != _request) return _request;
                if (null != _controller) return _controller.Request;
                throw new NullReferenceException("No request, or method to resolv a request, supplied to ResponseBroker.");
            }
        }
    }
}
