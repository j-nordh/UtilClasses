using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UtilClasses.Interfaces;
using UtilClasses.Extensions.Dictionaries;
namespace UtilClasses.Services.Api
{
    public abstract class CrudControllerBase<TKey, T> : CruControllerBase<TKey, T> where T : class
    {
        protected CrudControllerBase(IManager<TKey, T> man) : base(man)
        {
        }

        [Route("{id}")]
        [HttpDelete]
        [SwaggerOperation("Deletes a single object as specified by the supplied Id")]
        public virtual void Delete(TKey id) => _man.Delete(id);
    }
}