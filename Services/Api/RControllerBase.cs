using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UtilClasses.Interfaces;

namespace UtilClasses.Services.Api;

public abstract class RControllerBase<TKey, T> : Controller where T : class, IHasId<TKey>
{
    private readonly IAsyncRRepository<TKey, T> _repo;

    protected RControllerBase(IAsyncRRepository<TKey, T> repo)
    {
        _repo = repo;
    }
    [Route("")]
    [HttpGet]
    [SwaggerOperation("Returns all stored objects")]
    public virtual async Task<List<T>> Read() => await _repo.GetAsync();
    [Route("{id}")]
    [HttpGet]
    [SwaggerOperation("Returns a single object as specified by the supplied Id")]
    public virtual async Task<ActionResult<T>> Read(TKey id)
    {
        var o = await _repo.GetAsync(id);

        return o != null
            ? new OkObjectResult(o)
            : new NotFoundResult();
    }

    [Route("Refresh")]
    [HttpPost]
    [SwaggerOperation("Refreshes the objects from storage")]
    [ApiExplorerSettings(IgnoreApi = true)] 
    public virtual void Refresh() => _repo.Refresh();
}