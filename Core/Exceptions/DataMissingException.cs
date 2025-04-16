using System;

namespace UtilClasses.Core.Exceptions;

public class DataMissingException:Exception
{
    public DataMissingException(string message): base(message)
    { }
}