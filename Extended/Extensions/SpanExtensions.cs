using System;
using System.Collections.Generic;
using System.Text;

namespace ExtendedUtilClasses.Extensions
{
    public static class SpanExtensions
    {
        public static bool IsNullOrEmpty<T>(this Span<T> span) => null == span || span.IsEmpty;
    }
}
