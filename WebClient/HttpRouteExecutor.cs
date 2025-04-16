using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Strings;
using UtilClasses.WebClient.Extensions;

namespace UtilClasses.WebClient
{
    public static class HttpRouteExecutorExtensions
    {
        public static event Action<HttpRoute> PreExecute;
        private static readonly Dictionary<string, Uri> BaseAddressDict = new Dictionary<string, Uri>();
        private static Uri _defaultBaseUri;
        private static JsonSerializerSettings _settings;

        public static Task<T> Execute<T>(this HttpRoute<T> route) => Execute(route, null);
        public static Task<T> Execute<T>(this HttpRoute<T> route, string baseAddress)
        {
            PreExecute?.Invoke(route);
            var com = route.GetCom(baseAddress);
            switch (route.Method.ToUpper())
            {
                case "GET":
                    return com.Get<T>();
                case "POST":
                    return com.Post<T>();
                case "DELETE":
                    throw new InvalidOperationException("Cannot execute delete in a typed context.");
                case "PUT":
                    return com.Put<T>();
                default:
                    throw new ArgumentException($"Method '{route.Method}' is not supported");
            }
        }

        public static Task Execute(this HttpRoute route) => Execute(route, null);
        public static Task Execute(this HttpRoute route, string baseAddress)
        {
            PreExecute?.Invoke(route);
            var com = route.GetCom(baseAddress);
            switch (route.Method.ToUpper())
            {
                case "GET":
                    return com.Get();
                case "POST":
                    return com.Post();
                case "DELETE":
                    return com.Delete();
                case "PUT":
                    return com.Put();
                default:
                    throw new ArgumentException($"Method '{route.Method}' is not supported");
            }
        }
        public static void SetBaseAddress<T>(string address) where T : HttpRoute
        {
            var uri = new Uri(address);
            uri.ValidateAsBase();
            var key = typeof(T).Name;
            if (key.Contains("<"))
                key = key.Substring(0,key.IndexOf('<'));
            BaseAddressDict[key] = uri;
        }
        public static void SetBaseAddress(string address)
        {
            var uri = new Uri(address);
            uri.ValidateAsBase();
            _defaultBaseUri = uri;
        }
        public static void SetJsonSettings(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        private static HttpCom GetCom(this HttpRoute r, string baseAddress = null)
        {
            var uri = GetBase(r, baseAddress);
            uri.ValidateAsBase();
            return r.GetCom(uri);
        }

        private static Uri GetBase(HttpRoute r, string baseAddress = null)
        {
            if(baseAddress!= null) return new Uri(baseAddress);
            var key = r.GetType().Name.SubstringBefore("<").SubstringBefore("`");
            return BaseAddressDict.Maybe(key) ?? _defaultBaseUri;
        }


        private static HttpCom GetCom(this HttpRoute route, Uri baseAddress) =>
            new HttpCom(new Uri(baseAddress, route.Route).ToString())
                .WithJsonSettings(_settings)
                .WithQueryParams(route.ParameterList, p => p.Name, p => p.Value)
                .WithAuth(route.Auth)
                .WithBody(route.Body);
    }
}
