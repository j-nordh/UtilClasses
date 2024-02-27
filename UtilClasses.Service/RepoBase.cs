using System.Collections.Generic;
using System.Net.Http;
using MACS.Common;

namespace MACS.Service;

public abstract class RepoBase<TKey, T> where T : class
{
    private readonly string _baseUri;
    protected RestClient _client;
    protected RepoBase(IHttpClientFactory httpClientFactory, ServiceRegistry services, string baseUri)
    {
        _baseUri = baseUri;
        _client = new RestClient(httpClientFactory, services.Lookup(ServiceType.StorageService));
    }

    public virtual List<T> Get() => _client.Get<List<T>>(_baseUri);
    public virtual T Get(TKey key) => _client.Get<T>(_baseUri, key.ToString());
    public virtual void Refresh() => _client.Post($"{_baseUri}/Refresh");
}