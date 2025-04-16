using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Objects;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core;

public class Csv<T>
{
    private readonly string _delimiter;
    private readonly (string Name, Func<T, string> Extractor)[] _columns;
    StringBuilder _sb= new StringBuilder();
    public Csv(string delimiter, bool includeHeader, params (string name, Func<T, string> extractor)[] columns)
    {
        _delimiter = delimiter;
        _columns = columns;
        if (!includeHeader) return;
        _sb.AppendLine(_columns.Select(c=>c.Name).Join(delimiter));
    }

    public Csv<T> Add(T obj) => this.Do(()=>_sb.AppendLine(_columns.Select(c=>c.Extractor(obj)).Join(_delimiter)));

    public Csv<T> AddEmptyRow() => this.Do(() => _sb.AppendLine());
    public Csv<T> AddRaw(params string[] vals) => this.Do(() => _sb.AppendLine(vals.Join(_delimiter)));

    public Csv<T> AddRange(IEnumerable<T> objs) => this.Do(() => objs.ForEach(o=>Add(o)));

    public override string ToString() => _sb.ToString();
}