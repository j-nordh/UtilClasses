using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.WebClient
{
    public abstract class HttpRoute
    {
        public readonly List<RouteParameter> ParameterList;
        public string Route { get; }
        public string Method { get; set; }
        public object Body { get; set; }
        public bool IsAnonymous { get; set; }
        public string Auth { get; set; }
        public string Parameters
        {
            get { return string.Join("&", ParameterList.Select(kvp => $"{kvp.Name}={kvp.Value}").ToArray()); }
        }

        protected HttpRoute(string route)
        {
            Route = route;
            ParameterList = new List<RouteParameter>();
            Method = "GET";
        }

      
    }

    public class RouteParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public RouteParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class HttpRoute<T> : HttpRoute
    {
        public HttpRoute(string route) : base(route)
        {
        }
    }
}
