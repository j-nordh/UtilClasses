using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Core.CodeGeneration;
using UtilClasses.Core.Extensions.Objects;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core;

public static class IndentingStringBuilderExtensions
{
    public static IEnumerable<IAppendable> AsAppendable(this IEnumerable<string> strings) =>
        strings.Select(s => new StringAppendable(s));

    public static IndentingStringBuilder Inject(this IndentingStringBuilder sb,
        IEnumerable<Func<IndentingStringBuilder, IndentingStringBuilder>> fs)
    {
        foreach (var f in fs)
        {
            sb = f(sb);
        }

        return sb;
    }

    public static IndentingStringBuilder Inject(this IndentingStringBuilder sb,
        Func<IndentingStringBuilder, IndentingStringBuilder> f)
    {
        sb = f(sb);
        return sb;
    }

    public static IndentingStringBuilder Inject(this IndentingStringBuilder sb, IEnumerable<IInjector> injectors,
        Func<IInjector, Func<IndentingStringBuilder, IndentingStringBuilder>> f)
        => sb.Inject(injectors.Select(f));

    public static IndentingStringBuilder AppendAll(this IEnumerable<IAppendable> items, IndentingStringBuilder sb) =>
        sb.AppendObjects(items);

    public static IAppendable Then(this IAppendable a1, IAppendable a2) => new MultiAppendable(a1, a2);

    public static string Render(this IAppendable a1, string indent = "  ")
    {
        var sb = new IndentingStringBuilder(indent);
        sb.AppendObject(a1);
        return sb.ToString();
    }

    public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, string line,
        Func<FunctionBuilder, FunctionBuilder> cfg) =>
        sb.AppendObject(cfg(FromLine(line)));

    public static IndentingStringBuilder AppendInlineFunction(this IndentingStringBuilder sb, string declaration,
        string line) =>
        sb.AppendObject(FromLine(declaration).Inline(line));

    public static IndentingStringBuilder AppendInlineFunction(this IndentingStringBuilder sb, string declaration,
        string parameters, string line) =>
        sb.AppendObject(FromLine(declaration).WithParameter(parameters).Inline(line));

    public static IndentingStringBuilder AppendInlineFunction(this IndentingStringBuilder sb, string declaration,
        IEnumerable<string> parameters, string line) =>
        sb.AppendObject(FromLine(declaration).WithParameters(parameters).Inline(line));

    public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, bool predicate, string line,
        Func<FunctionBuilder, FunctionBuilder> cfg) => predicate ? sb.AppendObject(cfg(FromLine(line))) : sb;

    public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb,
        Func<FunctionBuilder, FunctionBuilder> cfg) =>
        sb.AppendObject(cfg(new FunctionBuilder()));

    public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, bool predicate, string line,
        string parameter, Func<FunctionBuilder, FunctionBuilder> cfg) =>
        predicate ? sb.AppendFunction(line, parameter, cfg) : sb;

    public static IndentingStringBuilder AppendFunction(this IndentingStringBuilder sb, string line, string parameter,
        Func<FunctionBuilder, FunctionBuilder> cfg) =>
        sb.AppendObject(cfg(FromLine(line).WithParameter(parameter)));

    public static IndentingStringBuilder AppendFunction<T>(this IndentingStringBuilder sb, string line,
        string parameter, IEnumerable<T> objs, Func<T, string> formatter) =>
        sb.AppendObject(FromLine(line).WithParameter(parameter).WithBuilder(() => sb.AppendObjects(objs, formatter)));

    public static IndentingStringBuilder AppendFunction<T>(this IndentingStringBuilder sb, string line,
        string parameter, IEnumerable<T> objs, Action<IndentingStringBuilder, T> appender) =>
        sb.AppendObject(FromLine(line).WithParameter(parameter).WithBuilder(() => sb.AppendObjects(objs, appender)));

    public static IndentingStringBuilder AppendForEach(this IndentingStringBuilder sb, string varName, string col,
        params string[] lines) => sb
        .AppendLines($"foreach (var {varName} in {col})", "{")
        .AppendLines(lines)
        .AppendLine("}");

    public static IndentingStringBuilder AppendForEach(this IndentingStringBuilder sb, string varName, string col,
        Func<IndentingStringBuilder, IndentingStringBuilder> f) =>
        f(sb.AppendLines($"foreach (var {varName} in {col})", "{")).AppendLine("}");

    public static void Add(this List<IAppendable> lst, string s) => lst.Add(new StringAppendable(s));

    private static FunctionBuilder FromLine(string line)
    {
        var ret = new FunctionBuilder();
        var parts = line?.SplitRemoveEmptyEntries(" ") ?? new string[] { };
        if (parts.Count() == 1)
            ret.Name = parts[0];
        if (parts.Count() == 2)
        {
            ret.Returns = parts[0];
            ret.Name = parts[1];
        }

        if (parts.Count() == 3)
        {
            ret.Modifier = parts[0];
            ret.Returns = parts[1];
            ret.Name = parts[2];
        }

        return ret;
    }


    public static FunctionBuilder WithModifier(this FunctionBuilder fb, string mod) => fb.Do(() => fb.Modifier = mod);
    public static FunctionBuilder Returning(this FunctionBuilder fb, string ret) => fb.Do(() => fb.Returns = ret);

    public static FunctionBuilder ReturningList(this FunctionBuilder fb) =>
        fb.Do(() => fb.Returns = $"List<{fb.Returns}>");

    public static FunctionBuilder ReturningList(this FunctionBuilder fb, string ret) =>
        fb.Do(() => fb.Returns = $"List<{ret}>");

    public static FunctionBuilder WithName(this FunctionBuilder fb, string name) => fb.Do(() => fb.Name = name);

    public static FunctionBuilder InsertParameter(this FunctionBuilder fb, string p) =>
        fb.Do(() => fb.Parameters.Insert(0, p));

    public static FunctionBuilder InsertParameter(this FunctionBuilder fb, int index, string p) =>
        fb.Do(() => fb.Parameters.Insert(index, p));

    public static FunctionBuilder WithParameter(this FunctionBuilder fb, string p) => fb.Do(() => fb.Parameters.Add(p));

    public static FunctionBuilder WithoutOptionalParameters(this FunctionBuilder fb) => fb.Do(() =>
        fb.Parameters = fb.Parameters.Select(p => p.SubstringBefore("=")).ToList());

    public static FunctionBuilder WithParameters(this FunctionBuilder fb, IEnumerable<string> ps) =>
        fb.Do(() => fb.Parameters.AddRange(ps));

    public static FunctionBuilder WithParameters(this FunctionBuilder fb, params string[] ps) =>
        fb.Do(() => fb.Parameters.AddRange(ps));

    public static FunctionBuilder WithBuilder(this FunctionBuilder fb,
        Func<IndentingStringBuilder, IndentingStringBuilder> f) => fb.Do(() => fb.BodyBuilders.Add(f));

    public static FunctionBuilder WithBuilder(this FunctionBuilder fb, Func<IndentingStringBuilder> f) =>
        fb.Do(() => fb.BodyBuilders.Add(_ => f()));

    public static FunctionBuilder WithBuilder(this FunctionBuilder fb, Action<IndentingStringBuilder> a) => fb.Do(() =>
        fb.BodyBuilders.Add(isb =>
        {
            a(isb);
            return isb;
        }));

    public static FunctionBuilder WithBuilder(this FunctionBuilder fb, Action a) => fb.Do(() =>
        fb.BodyBuilders.Add(isb =>
        {
            a();
            return isb;
        }));

    public static FunctionBuilder ClearBuilders(this FunctionBuilder fb) => fb.Do(() => fb.BodyBuilders.Clear());

    public static FunctionBuilder With<T>(this FunctionBuilder fb, IEnumerable<T> objects, Func<T, string> formatter) =>
        fb.WithBuilder(sb => sb.AppendObjects(objects, formatter));

    public static FunctionBuilder With<T>(this FunctionBuilder fb, IEnumerable<T> objects,
        Action<IndentingStringBuilder, T> appender) => fb.WithBuilder(sb => sb.AppendObjects(objects, appender));

    public static FunctionBuilder With(this FunctionBuilder fb, params string[] lines) =>
        fb.WithBuilder(sb => sb.AppendLines(lines));


    public static FunctionBuilder WithObj(this FunctionBuilder fb, string type) =>
        fb.WithParameter($"{type} obj").Do(() => fb.HasObj = true);

    public static FunctionBuilder WithObjs(this FunctionBuilder fb, string type) =>
        fb.WithParameter($"IEnumerable<{type}> objs").Do(() => fb.HasObjs = true);

    public static FunctionBuilder Inline(this FunctionBuilder fb) => fb.Do(() => fb.Inline = true);

    public static FunctionBuilder Inline(this FunctionBuilder fb, string line) =>
        fb.ClearBuilders().Inline().With(line);

    public static FunctionBuilder Inline(this FunctionBuilder fb, params string[] lines) => fb
        .ClearBuilders()
        .Inline()
        .WithBuilder(b => b
            .AppendLine(lines[0])
            .Maybe(lines.Count() > 1,
                () => b.Indent().AppendLines(lines.Skip(1)).Outdent()));

    public static IEnumerable<FunctionBuilder> Split(this FunctionBuilder fb,
        params Func<FunctionBuilder, FunctionBuilder>[] cfgs)
    {
        foreach (var cfg in cfgs)
        {
            yield return cfg(new FunctionBuilder(fb));
        }
    }

    public static IndentingStringBuilder WithHeader(this IndentingStringBuilder sb, string header,
        Action<IndentingStringBuilder> appender)
    {
        sb.AppendLine(header)
            .Indent();
        appender(sb);
        return sb.Outdent();
    }

    public static IndentingStringBuilder WithHeader(this IndentingStringBuilder sb, bool predicate, string header,
        Action<IndentingStringBuilder> appender)
        => predicate
            ? sb.WithHeader(header, appender)
            : sb;
}