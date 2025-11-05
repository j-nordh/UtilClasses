using System;
using System.Diagnostics;
using System.Linq;
using UtilClasses.Core.Extensions.Strings;
using UtilClasses.Core.Extensions.Types;

namespace UtilClasses.Core.Extensions.Exceptions;

public static class ExceptionExtensions
{
    public static string DeepToString(this Exception? ex)
    {
        if (null == ex) return "";
        var sb = new IndentingStringBuilder("    ");
        sb.AppendException(ex);
        return sb.ToString();
    }

    public static IndentingStringBuilder AppendException(this IndentingStringBuilder sb, Exception? ex) =>
        ex is null
            ? sb
            : sb
                .AppendLine($"{ex.GetType().SaneName()}: {ex.Message}")
                .WithHeader(ex.StackTrace.IsNotNullOrEmpty(), "StackTrace: ", x => x
                    .AppendLines(ex.StackTrace.SplitRemoveEmptyEntries(Environment.NewLine)))
                .WithHeader(ex.Data.Count > 0, "Data: ", x => x
                    .AppendObjects(ex.Data.Keys.Cast<object>(), key => $"{key}: {ex.Data[key]}"))
                .Indent(y => y.AppendException(ex.InnerException!));

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
    public TracedException(string message, Exception inner, string stackTrace) : base(message, inner)
    {
        StackTrace = stackTrace;
    }

    public override string StackTrace { get; }
}