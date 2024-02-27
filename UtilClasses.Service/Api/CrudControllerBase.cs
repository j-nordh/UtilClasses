using System;
using System.Collections.Generic;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Swashbuckle.AspNetCore.Annotations;

namespace MACS.SourceService.Api
{
    public abstract class RControllerBase<T, TMan> : RControllerBase<T, Guid, TMan> where TMan : class, IRRepository<T> where T : class, IHasGuid
    {
        protected RControllerBase(TMan man) : base(man)
        {
        }
    }

    public abstract class RControllerBase<T, TKey, TMan> : Controller where TMan : class, IRRepository<TKey, T> where T : class
    {
        protected readonly TMan _man;

        protected RControllerBase(TMan man)
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

    public abstract class CruControllerBase<T, TMan> : RControllerBase<T,TMan> where TMan : class, ICruRepository<T> where T : class, IHasGuid
    {
        protected CruControllerBase(TMan man) : base(man)
        {
        }
        [Route("")]
        [HttpPost]
        [SwaggerOperation("Creates a new object from the supplied configuration")]
        public virtual void Create([FromBody] T obj) => _man.Add(obj);
        [Route("")]
        [HttpPut]
        [SwaggerOperation("Updates a single object", " The object is specified by the supplied Id and the new object replaces the old completely")]
        public virtual void Update(Guid id, [FromBody] T obj) => _man.Put(id, obj);
    }
    public abstract class CrudControllerBase<T, TMan> : CruControllerBase<T,TMan> where TMan : class, ICrudRepository<T> where T : class, IHasGuid
    {
        protected CrudControllerBase(TMan man) : base(man)
        {
        }




        [Route("{id}")]
        [HttpDelete]
        [SwaggerOperation("Deletes a single object as specified by the supplied Id")]
        public virtual void Delete(Guid id) => _man.Delete(id);




        //protected void Describe(string name, HttpVerbs verb, string expectsBody, string returns, string description, string route, params (string Name, string Type, bool Optional)[] ps)
        //{
        //    if (route.IsNotNullOrEmpty())
        //        route = route.Trim('/').Insert(0, "/");
        //    _descriptors.Add(new VerbDescriptor()
        //    {
        //        Description = description,
        //        FromBody = _stringProvider.DescribeType(expectsBody),
        //        Name = name,
        //        Route = $"/{_route}{route}",
        //        Returns = _stringProvider.DescribeType(returns),
        //        Verb = verb.ToString().ToUpper(),
        //        Parameters = ps.Any() ? ps.Select(t => new VerbParameterDescriptor() { Name = t.Name, Type = t.Type, Optional = t.Optional }).ToList() : new List<VerbParameterDescriptor>()
        //    });
        //}

    }
}

public static class CrudRepositoryExtensions
{
    public static bool MaybeGet<T>(this IRRepository<T> repo, Guid id, out T res) where T : class, IHasGuid
    {
        try
        {
            res = repo.Get(id);
            return res != null;
        }
        catch
        {
            res = null;
            return false;
        }
    }
    public static bool MaybeGet<TKey, T>(this IRRepository<TKey, T> repo, TKey id, out T res) where T : class
    {
        try
        {
            res = repo.Get(id);
            return res != null;
        }
        catch
        {
            res = null;
            return false;
        }
    }
}