using System;
using System.Collections.Generic;

namespace UtilClasses.Core.Extensions.Csvs;

public static class CsvExtensions
{
    public static string ToCsv<T>(this IEnumerable<T> objs, string delimiter, bool withHeader, params (string name, Func<T, string>)[] columns)
        => new Csv<T>(delimiter, withHeader, columns).AddRange(objs).ToString();
    public static string ToCsv<T>(this IEnumerable<T> objs, params (string name, Func<T, string>)[] columns) => objs.ToCsv(";", true, columns);
}