using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Strings;
using UtilClasses.Json;
using Microsoft.AspNetCore.Http.Extensions;
using UtilClasses.Core;

namespace UtilClasses.WebClient;

public class RestClient
{
    private IHttpClientFactory _httpClientFactory;
    private readonly string _host;
    private readonly int _port;

    public RestClient(IHttpClientFactory httpClientFactory, string host, int port)
    {
        _httpClientFactory = httpClientFactory;
        _host = host;
        _port = port;
    }
    protected HttpClient GetClient()
    {
        var c = _httpClientFactory.CreateClient();
        c.BaseAddress = GetUri();
        c.DefaultRequestHeaders.Add("Accept", "application/json");
        c.DefaultRequestHeaders.Add("User-Agent", "RigService");
        return c;
    }

    //public Uri GetUri() => new($"https://{_addressInfo.Address}:{_addressInfo.Port}");
    public Uri GetUri() => new($"http://{_host}:{_port}");

    public async Task<HttpResponseMessage> PutAsyncQuery(string uri, string id) =>
        await PutAsyncQuery(uri, ("id", id));
    public async Task<HttpResponseMessage> PutAsyncQuery(string uri, params (string Key, string Value)[] query)
    {
        var c = GetClient();
        if (null == c) return null;
        if (query.Any())
        {
            var q = new QueryBuilder(query.Select(t => new KeyValuePair<string, string>(t.Key, t.Value)));
            var builder = new UriBuilder(GetUri())
            {
                Query = q.ToString(),
                Path = uri
            };
            uri = builder.ToString();
        }

        var res = await c.PutAsync(uri, null);
        return res;
    }
    public async Task<HttpResponseMessage> PutAsync(string uri, string content)
    {
        var c = GetClient();
        if (null == c) return null;

        var res = null == content
            ? await c.PutAsync(uri, null)
            : await c.PutAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
        return res;

    }
    public async Task<HttpResponseMessage> PutAsync(string uri, object o) => await PutAsync(uri, JsonUtil.Serialize(o));
    #region Post

    public async Task<HttpResponseMessage> PostAsync(string uri) => await PostAsync(uri, null);
    public async Task<HttpResponseMessage> PostAsync(string uri, object o) => await PostAsync(uri, JsonUtil.Serialize(o));
    public async Task<T> PostAsync<T>(string uri, object o)
    {
        var res = await PostAsync(uri, JsonUtil.Serialize(o));
        return res?.IsSuccessStatusCode ?? false
            ? JsonUtil.Get<T>(await res.Content.ReadAsStringAsync())
            : await ThrowAsync<T>($"Failed to Post to {uri}", res);
    }

    public async Task<HttpResponseMessage> PostAsync(string uri, string content)
    {
        var c = GetClient();
        if (null == c) return null;

        return null == content
            ? await c.PostAsync(uri, null)
            : await c.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
    }

    public HttpResponseMessage Post(string uri) => Post(uri, null);
    public HttpResponseMessage Post(string uri, object o) => Post(uri, JsonUtil.Serialize(o));
    public HttpResponseMessage Post(string uri, string content, string mediaType = "application/json")
    {
        var c = GetClient();
        if (null == c) return null;
        var request = new HttpRequestMessage(HttpMethod.Post, GetUri(uri));
        if (content.IsNotNullOrEmpty())
            request.Content = new StringContent(content, Encoding.UTF8, mediaType);
        return c.Send(request);

    }

    #endregion

    #region Get

    public async Task<T2> GetAsync<T2>(string uri, string id) where T2 : class => await GetAsync<T2>(uri, ("id", id));
    public async Task<T2> GetAsync<T2>(string uri, params (string Key, string Value)[] query) where T2 : class
    {
        var c = GetClient();
        if (null == c) return null;
        var res = await c.GetAsync(GetUri(uri, query));
        if (res.IsSuccessStatusCode)
            return JsonUtil.Get<T2>(await res.Content.ReadAsStringAsync());

        return await ThrowAsync<T2>($"Get failed for {uri}", res);
    }


    public T Get<T>(string uri, string id) where T : class => Get<T>(uri, ("id", id));
    public T Get<T>(string uri, params (string Key, string Value)[] query)
    {
        var c = GetClient();
        var request = new HttpRequestMessage(HttpMethod.Get, GetUri(uri, query));
        var res = c.Send(request);
        if (res.IsSuccessStatusCode)
        {
            using var rdr = new StreamReader(res.Content.ReadAsStream());
            return JsonUtil.Get<T>(rdr.ReadToEnd());
        }

        return Throw<T>($"Get failed for {uri}", res);
    }
    public async Task<T2?> GetAsyncValue<T2>(string uri, Action<HttpClient> configure = null) where T2 : struct
    {
        var c = GetClient();
        if (null == c) return null;
        configure?.Invoke(c);
        var res = await c.GetAsync(uri);
        if (res.IsSuccessStatusCode)
            return JsonUtil.Get<T2>(await res.Content.ReadAsStringAsync());

        return await ThrowAsync<T2>($"Get failed for {uri}", res);
    }

    #endregion

    private string GetUri(string uri, params (string Key, string Value)[] query)
    {
        if (query.Any())
        {
            var q = new QueryBuilder(query.Select(t => new KeyValuePair<string, string>(t.Key, t.Value)));
            var builder = new UriBuilder(GetUri())
            {
                Query = q.ToString(),
                Path = uri
            };
            uri = builder.ToString();
        }
        return uri;
    }
    private async Task<TRes?> ThrowAsync<TRes>(string message, HttpResponseMessage res)
    {
        var body = await res.Content.ReadAsStringAsync();
        if (JsonUtil.TryGet<ExceptionWrapper>(body, out var wrapper))
            throw new Exception(message, wrapper.Unwrap());

        throw new Exception($"{message}.\r\n{body}");
    }
    private TRes Throw<TRes>(string message, HttpResponseMessage res)
    {
        using var rdr = new StreamReader(res.Content.ReadAsStreamAsync().Result);
        var body = rdr.ReadToEnd();
        if (JsonUtil.TryGet<ExceptionWrapper>(body, out var wrapper))
            throw new Exception(message, wrapper.Unwrap());

        var ex = new HttpRequestException($"{message}.\r\n{body}", null, res.StatusCode);
        throw ex;
    }
}