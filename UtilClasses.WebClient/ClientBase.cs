using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Strings;
using UtilClasses.WebClient.Extensions;

namespace UtilClasses.WebClient
{
    public abstract class ClientBase
    {
        //public RefreshingTokenHandler TokenHandler { get; set; }
        public ClientBase(string baseAddress)
        {
            _defaultBaseUri = new Uri(baseAddress).ValidateAsBase();
        }
        public event Action<HttpRoute> PreExecute;
        private readonly Dictionary<string, Uri> BaseAddressDict = new Dictionary<string, Uri>();
        private Uri _defaultBaseUri;
        protected async Task<T> Execute<T>(HttpRoute<T> route)
        {
            PreExecute?.Invoke(route);
            var com = await GetCom(route);
            switch (route.Method.ToUpper())
            {
                case "GET":
                    return await com.Get<T>();
                case "POST":
                    return await com.Post<T>();
                case "DELETE":
                    throw new InvalidOperationException("Cannot execute delete in a typed context.");
                case "PUT":
                    return await com.Put<T>();
                default:
                    throw new ArgumentException($"Method '{route.Method}' is not supported");
            }
        }

        protected async Task Execute(HttpRoute route)
        {
            PreExecute?.Invoke(route);
            var com = await GetCom(route);
            switch (route.Method.ToUpper())
            {
                case "GET":
                    await com.Get();
                    return;
                case "POST":
                    await com.Post();
                    return;
                case "DELETE":
                    await com.Delete();
                    return;
                case "PUT":
                    await com.Put();
                    return;
                default:
                    throw new ArgumentException($"Method '{route.Method}' is not supported");
            }
        }
        public void SetBaseAddress<T>(string address) where T : HttpRoute
        {
            var uri = new Uri(address);
            uri.ValidateAsBase();
            var key = typeof(T).Name;
            if (key.Contains("<"))
                key = key.Substring(0, key.IndexOf('<'));
            BaseAddressDict[key] = uri;
        }
        public void SetBaseAddress(string address)
        {
            var uri = new Uri(address);
            uri.ValidateAsBase();
            _defaultBaseUri = uri;
        }

        private Task<HttpCom> GetCom(HttpRoute r) => GetCom(r, GetBase(r));

        private Uri GetBase(HttpRoute r, string baseAddress = null)
        {
            if (baseAddress != null) return new Uri(baseAddress);
            var key = r.GetType().Name.SubstringBefore("<").SubstringBefore("`");
            return BaseAddressDict.Maybe(key) ?? _defaultBaseUri;
        }

        private async Task<HttpCom> GetCom(HttpRoute route, Uri baseAddress)
        {
            await Task.FromResult(0);
            var ret = new HttpCom(new Uri(baseAddress, route.Route).ToString())
                        .WithQueryParams(route.ParameterList, p => p.Name, p => p.Value)
                        .WithBody(route.Body);
            //if (!route.IsAnonymous && TokenHandler != null)
            //    ret.WithAuth($"Bearer {await TokenHandler.Access.Token()}");
            return ret;
        }
    }
}
