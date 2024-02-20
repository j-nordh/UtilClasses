using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Plugins
{
    public static class Extensions
    {
        public static void Add(this List<ParameterMetaData> lst, string name, string description, bool mandatory = false)
        {
            lst.Add(new ParameterMetaData { Name = name, Description = description, Mandatory = mandatory });
        }
    }
}
