using System;
using System.Collections.Generic;

namespace UtilClasses
{
    public class DictionaryOic<T>() : Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
}
