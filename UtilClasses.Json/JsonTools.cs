using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UtilClasses.Extensions.Assemblies;

namespace UtilClasses.Json
{
    public static class JsonTools
    {
        public static T JsonResource<T>(this Assembly ass, string id)=>
            JsonConvert.DeserializeObject<T>(ass.GetResourceString(id));

        public static T FromFile<T>(string path, Encoding enc = null)
        {
            enc ??= Encoding.UTF8;
            var str = File.ReadAllText(path, enc);
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static void ToFile(string path, object o, Encoding enc = null)
        {
            enc ??= Encoding.UTF8;
            File.WriteAllText(path, JsonConvert.SerializeObject(o, Formatting.Indented), enc);
        }
    }
}
