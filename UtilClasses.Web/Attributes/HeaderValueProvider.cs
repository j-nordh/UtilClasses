using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace UtilClasses.Web.Attributes
{
    public class FromHeaderAttribute : ModelBinderAttribute
    {
        public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            return base.GetValueProviderFactories(configuration).OfType<HeaderValueProviderFactory>();
        }
    }

    public class HeaderValueProvider: IValueProvider
    {
        private readonly HttpRequestMessage _requestMessage;

        public HeaderValueProvider(HttpRequestMessage requestMessage)
        {
            _requestMessage = requestMessage;
        }

        public bool ContainsPrefix(string prefix) => _requestMessage.Headers.Contains(prefix);

        public ValueProviderResult GetValue(string key)
        {
            
            IEnumerable<string> values;
            if (!_requestMessage.Headers.TryGetValues(key, out values)) return null;
            var val = values.FirstOrDefault();
            return null != val ? new ValueProviderResult(val, val, CultureInfo.InvariantCulture) : null;
        }
    }

    public class HeaderValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext) =>
            new HeaderValueProvider(actionContext.Request);
    }

    
}
