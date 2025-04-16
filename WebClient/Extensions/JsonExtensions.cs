using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.WebClient.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object o) => Newtonsoft.Json.JsonConvert.SerializeObject(o);
    }
}
