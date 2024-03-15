using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UtilClasses.Interfaces;

namespace UtilClasses.Services.Api;

public abstract class CruControllerBase<TKey, T> : RControllerBase<TKey, T> where  T : class
{
    protected CruControllerBase(IManager<TKey, T> man) : base(man)
    {
    }
    [Route("")]
    [HttpPost]
    [SwaggerOperation("Creates a new object from the supplied configuration")]
    public virtual void Create([FromBody] T obj) => _man.Add(obj);
    [Route("")]
    [HttpPut]
    [SwaggerOperation("Updates a single object", " The object is specified by the supplied Id and the new object replaces the old completely")]
    public virtual void Update(TKey id, [FromBody] T obj) => _man.Put(id, obj);
}