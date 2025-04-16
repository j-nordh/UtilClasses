using System;
using System.Collections.Generic;

namespace UtilClasses.Core;

public class DictionaryOic<T>() : Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);