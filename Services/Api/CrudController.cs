using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UtilClasses.Interfaces;
namespace UtilClasses.Services.Api
{
    public interface ICrudController<TKey, T> where T : class, IHasId<TKey>
    {
        Task Delete(TKey id);
        Task<List<T>> Read();
        Task<ActionResult<T>> Read(TKey id);
        Task Create([FromBody] T obj);
        Task Update(TKey id, [FromBody] T obj);
    }

    public class CrudController<TKey, T> : CruControllerBase<TKey, T>, ICrudController<TKey, T> where T : class, IHasId<TKey>
    {
        private readonly IAsyncCrudRepository<TKey, T> _man;

        public CrudController(IAsyncCrudRepository<TKey, T> man) : base(man)
        {
            _man = man;
        }

        [Route("{id}")]
        [HttpDelete]
        [SwaggerOperation("Deletes a single object as specified by the supplied Id")]
        public virtual async Task Delete(TKey id) => await _man.Delete(id);
    }
}