using System;
using System.Collections.Generic;
using System.Text;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.WebClient.Extensions
{
    public static class HttpRouteExtensions
    {
        public static T Anonymous<T>(this T route) where T : HttpRoute => route.Do(r => r.IsAnonymous = true);
        public static HttpRoute<T> Anonymous<T>(this HttpRoute<T> route) => route.Do(r => r.IsAnonymous = true);

        public static  T AddParameter<T>(this T route, string name, string value) where T: HttpRoute => route.Do(r=>r.ParameterList.Add(name, value));

        public static T AddParameter<T>(this T route, string name, object o) where T : HttpRoute => route.Do(r => r.ParameterList.Add(name, o.ToString()));
        public static T SetMethod<T>(this T route, string method ) where T : HttpRoute => route.Do(r => r.Method=method);
        public static T SetBody<T>(this T route, object body) where T : HttpRoute => route.Do(r => r.Body = body);
        public static void Add(this List<RouteParameter> ps, string name, object o)
        {
            if (null == name || null == o) return;
            ps.Add(new RouteParameter(name, o.ToString()));
        }

        public static T WithBasicAuth<T>(this T route, string username, string password) where T : HttpRoute
        {
            var cred = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            route.Auth = $"Basic {cred}";
            return route;
        }

        public static T WithToken<T>(this T route, string token) where T : HttpRoute => route.Do(r=>r.Auth=$"Bearer {token}");

        public static T WithHeader<T>(this T route, string header, string value) where T: HttpRoute
        {
            if(!header.EqualsOic("Authorization")) throw new ArgumentOutOfRangeException("Only Authorization supported...");
            route.Auth = value;
            return route;
        }
    }
    static class RecsClientExtensions
    {
        public static Uri ValidateAsBase(this Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("The supplied base uri is not absolute! (A.K.A Where is that server????)");
            return uri;
        }
    }
}
