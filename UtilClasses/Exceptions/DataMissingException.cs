using System;
using System.Collections.Generic;
using System.Text;

namespace UtilClasses.Exceptions
{
    public class DataMissingException:Exception
    {
        public DataMissingException(string message): base(message)
        { }
    }
}
