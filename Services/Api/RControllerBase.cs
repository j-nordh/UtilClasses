using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UtilClasses.Interfaces;

namespace UtilClasses.Services.Api;

public abstract class RControllerBase<TKey, T> : Controller where   T : class
{
    protected readonly IManager<TKey, T> _man;

    protected RControllerBase (IManager<TKey, T> man)
    {
        _man = man;
    }
    [Route("")]
    [HttpGet]
    [SwaggerOperation("Returns all stored objects")]
    public virtual List<T> Read() => _man.Get();
    [Route("{id}")]
    [HttpGet]
    [SwaggerOperation("Returns a single object as specified by the supplied Id")]
    public virtual ActionResult<T> Read(TKey id) =>
        _man.MaybeGet(id, out var ret)
            ? new OkObjectResult(ret)
            : new NotFoundResult();

    [Route("Refresh")]
    [HttpPost]
    [SwaggerOperation("Refreshes the objects from storage")]
    public virtual void Refresh() => _man.Refresh();
}