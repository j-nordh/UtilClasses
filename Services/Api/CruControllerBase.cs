using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UtilClasses.Interfaces;

namespace UtilClasses.Services.Api;

public abstract class CruControllerBase<TKey, T> : RControllerBase<TKey, T> where  T : class, IHasId<TKey>
{
    private readonly IAsyncCruRepository<TKey, T> _repo;

    protected CruControllerBase(IAsyncCruRepository<TKey, T> repo) : base(repo)
    {
        _repo = repo;
    }
    [Route("")]
    [HttpPost]
    [SwaggerOperation("Creates a new object from the supplied configuration")]
    public virtual async Task Create([FromBody] T obj) => await _repo.Add(obj);
    [Route("{id}")]
    [HttpPut]
    [SwaggerOperation("Updates a single object", " The object is specified by the supplied Id and the new object replaces the old completely")]
    public virtual async Task Update(TKey id, [FromBody] T obj) => await _repo.Put(id, obj);
}