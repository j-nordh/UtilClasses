using System;
using System.Collections.Generic;
using System.Text;

namespace UtilClasses.Core.Extensions.StringBuilders;

public static class StringBuilderExtensions
{
    public static void AppendException(this StringBuilder strb, System.Exception ex, string indent="")
    {
        strb.Append(indent).Append("Error Message: ").AppendLine(ex.Message);
        strb.Append(indent).Append("Type: ").AppendLine(ex.GetType().ToString());
        foreach (var key in ex.Data.Keys)
        {
            strb.Append(key).Append(": ").AppendLine(ex.Data[key]);
        }
        strb.Append(indent).AppendLine("Stacktrace: ");
        if (!string.IsNullOrEmpty(ex.StackTrace))
        {
            foreach (var line in ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                strb.Append(indent + "  ").AppendLine(line);
            }
        }
        if (ex.InnerException == null) return;

        strb.AppendLine();
        AppendException(strb, ex.InnerException, indent + "    ");
    }

    public static void AppendLine(this StringBuilder strb, object o)
    {
        strb.AppendLine(o.ToString());
    }

    public static StringBuilder AppendLines(this StringBuilder strb, IEnumerable<string> strings)
    {
        foreach (var s in strings)
        {
            strb.AppendLine(s);
        }
        return strb;
    }

    public static StringBuilder Tab(this StringBuilder strb) => strb.Append("\t");
}