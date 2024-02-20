using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;
using Uri = System.Uri;

namespace UtilClasses.WebClient
{
    public class HttpCom
    {
        public JsonSerializerSettings JsonSettings { get; set; }
        public string Auth { get; set; }
        public string Body { get; set; }
        public string MediaType { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Func<string, string> Filter { get; set; }
        public string ResponseContent { get; private set; }
        public string Address { get; set; }
        public Uri Uri => BuildUri();

        public Dictionary<string, string> Query { get; }
        public int? HttpTimeout { get; set; }

        public HttpCom(string address)
        {
            Address = address;
            Query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            MediaType = "application/json";
            JsonSettings = new JsonSerializerSettings();
            Headers = new Dictionary<string, string>();
            Filter = s => s;
        }

        

        public async Task<T> Get<T>() =>await Perform<T>(c => c.GetAsync(BuildUri()));
        public async Task<string> Get() => await Perform(c => c.GetAsync(BuildUri()));

        public async Task<T> Post<T>() => await Perform<T>(DoPost);

        public async Task<string> Post() => await Perform(DoPost);
        public async Task Delete() => await Perform(DoDelete);
        public async Task Put() => await Perform(DoPut);
        public async Task<T> Put<T>() => await Perform<T>(DoPut);

        private async Task<HttpResponseMessage> DoPost(HttpClient c) =>
            await c.PostAsync(BuildUri(), new StringContent(Body, Encoding.UTF8, MediaType));

        private async Task<HttpResponseMessage> DoDelete(HttpClient c) => await c.DeleteAsync(BuildUri());

        private async Task<HttpResponseMessage> DoPut(HttpClient c) =>
            await c.PutAsync(BuildUri(), new StringContent(Body, Encoding.UTF8, MediaType));

        private async Task<T> Perform<T>(Func<HttpClient, Task<HttpResponseMessage>> f)
        {
            var res = await Perform(f);
            if(null == res) return default;
            return res.IsJson() 
                ? JsonConvert.DeserializeObject<T>(res, JsonSettings) 
                : res.As<T>();
        }

        private async Task<string> Perform(Func<HttpClient, Task<HttpResponseMessage>> f)
        {
            var client = new HttpClient();

            if (HttpTimeout.HasValue) 
                client.Timeout = TimeSpan.FromSeconds(HttpTimeout.Value);

            if (Auth.IsNotNullOrEmpty())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Auth.SubstringBefore(" "), Auth.SubstringAfter(" "));
            }

            Headers.ForEach(h => client.DefaultRequestHeaders.Add(h.Key, h.Value));
            
            var res = await f(client);
            ResponseContent = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
            {
                ResponseContent = Filter(ResponseContent);
                return ResponseContent;
            }   

            var ex = GetException(res, ResponseContent);
            if (ResponseContent.IsNotNullOrEmpty())
                ex.Data.Add("data", ResponseContent);

            throw ex;
        }

        private Exception GetException(HttpResponseMessage res, string content)
        {
            var msg =
                $"HttpCom call failed with StatusCode: {res.StatusCode}\r\nReason: {res.ReasonPhrase}\r\nMessage: {content}";
            switch (res.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized: return new UnauthorizedAccessException(msg);
                default: return new Exception(msg);
            }
        }
        
        private Uri BuildUri()
        {
            //return new Uri(@"https://c45c1020281ed936e5aa1bcccbb27673.m.pipedream.net");
            return new Uri(new IndentingStringBuilder("")
                .Append(Address)
                .Maybe(Query.Any(),
                    sb => sb.Append("?")
                        .Append(Query.Select(p => $"{p.Key}={p.Value}").Join("&"))).ToString());
        }
    }
    public static class HttpComExtensions
    {
        public static HttpCom WithJsonSettings(this HttpCom com, JsonSerializerSettings settings) => com.Do(() => com.JsonSettings= settings);
        public static HttpCom WithAddress(this HttpCom com, string address) => com.Do(() => com.Address = address);
        public static HttpCom WithAuth(this HttpCom com, string token) => com.Do(() => com.Auth = token);
        public static HttpCom WithQueryParam(this HttpCom com, string key, string val) => com.Do(() => com.Query[key] = val);
        public static HttpCom WithHeader(this HttpCom com, string key, string val) => com.Do(() => com.Headers[key] = val);
        public static HttpCom WithQueryParams<T>(this HttpCom com, IEnumerable<T> items, Func<T, string> keyFunc, Func<T, string> valFunc) =>
            com.Do(() => items.ForEach(i => com.WithQueryParam(keyFunc(i), valFunc(i))));
        public static HttpCom WithQueryParams(this HttpCom com, IEnumerable<(string key, string val)> kvs) => com.Do(() => kvs.ForEach(kv => com.WithQueryParam(kv.key, kv.val)));
        public static HttpCom WithBody(this HttpCom com, object content) => com.Do(() => com.Body = content as string ?? JsonConvert.SerializeObject(content, com.JsonSettings));
        public static HttpCom WithMediaType(this HttpCom com, string mediatype) => com.Do(() => com.MediaType = mediatype);
        public static HttpCom WithFilter(this HttpCom com, Func<string, string> filter) => com.Do(() => com.Filter = filter);
        public static HttpCom WithKeepAlive(this HttpCom com, bool keepAlive = true)
        {
            if (!keepAlive && com.Headers.ContainsKey("Connection"))
            {
                com.Headers.Remove("Connection");
                return com;
            }

            com.Headers["Connection"] = "keep-alive";
            return com;
        }

        public static HttpCom WithHttpTimeout(this HttpCom com, int? timeout) => com.Do(() => com.HttpTimeout = timeout);

    }
}
