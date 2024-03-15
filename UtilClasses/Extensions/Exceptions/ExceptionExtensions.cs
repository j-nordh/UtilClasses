using System;
using System.Diagnostics;
using System.Linq;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Extensions.Exceptions
{
    public static class ExceptionExtensions
    {
        public static string DeepToString(this Exception ex)
        {
            if (null == ex) return "";
            var sb= new IndentingStringBuilder("    ");
            sb.AppendException(ex);
            return sb.ToString();
        }

        public static IndentingStringBuilder AppendException(this IndentingStringBuilder sb, Exception ex) => sb
            .AppendLine($"Error Message: {ex.Message}")
            .AppendLine($"Type: {ex.GetType()}")
            .Maybe(ex.StackTrace.IsNotNullOrEmpty(), x => x
                .AppendLine("StackTrace: ")
                .AppendLines(ex.StackTrace.SplitREE(Environment.NewLine)))
            .Maybe(ex.Data.Count > 0, x => x
                .AppendLine("Data: ").Indent()
                .AppendObjects(ex.Data.Keys.Cast<object>(), key => $"{key}: {ex.Data[key]}")).Outdent()
            .Maybe(ex.InnerException != null, x => x
                .Indent(y => y
                    .AppendException(ex.InnerException)));

        public static void Throw(this Exception ex)
        {
            throw ex;
        }

        public static string ToFullString(this StackFrame sf)
        {
            // at RadarCommander.ComPortController.<List>d__9.MoveNext() in C:\Source\WirelessCom\RadarCommander\RadarCommander\ComPortController.cs:line 95
            var file = sf.GetFileName();
            if (null != file && file.Length > 5)
                file = " in " + file;
            var l = sf.GetFileLineNumber();
            var line = l <= 0 ? "" : $":line {l}";
            return $"at {sf.GetMethod()}{file}{line}";
        }
        public static string ToFullString(this StackTrace sf) => 
            new IndentingStringBuilder("  ")
                .AppendLines(sf.GetFrames()?.Select(ToFullString))
                .ToString();

        public static Type RealType(this Exception e)
        {
            if (e is AggregateException ae)
                return ae.InnerException.RealType();
            return e.GetType();
        }
    }
    public class TracedException : Exception
    {
        public TracedException(string message, Exception inner, string stackTrace): base(message, inner)
        {
            StackTrace = stackTrace;
        }
        public override string StackTrace { get; }
    }
}
