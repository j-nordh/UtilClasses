using System;

namespace UtilClasses
{
    public class ExceptionWrapper
    {
        public string Title { get; set; }
        public string StackTrace { get; set; }
        public ExceptionWrapper InnerException { get; set; }

        public static ExceptionWrapper From(Exception e)=>null == e
        ? null
        : new ()
        {
            Title = e.Message,
            StackTrace = e.StackTrace,
            InnerException = From(e)
        };

        public Exception Unwrap() => new Exception(Title, InnerException?.Unwrap());
}
    /*    private ExceptionWrapper GetException(Exception e) => null == e
        ? null
        : new()
        {
            Title = e.Message,
            StackTrace = e.StackTrace,
            InnerException = GetException(e.InnerException)
        };*/
}
