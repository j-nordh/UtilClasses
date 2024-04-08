using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ExtendedUtilClasses.Extensions.DateTimes;
using UtilClasses.Extensions.Bytes;
using UtilClasses.Extensions.DateTimes;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace ExtendedUtilClasses;

/*
 * Stolen with pride and adapted from:
 * https://github.com/he-dev/reusable/blob/dev/Reusable.Cryptography/src/FingerprintBuilder.cs
 *
 * Changes:
 *  * Everything serializes to string to avoid complexity
 *  * Uses a HashAlgorithm instead of more complex, generic method
 */
public class FingerprintBuilder<T>
{
    private class Extractor
    {
        private readonly Func<T, string> _f;

        public string Name { get; }

        public Extractor(string prop, Func<T, string> f)
        {
            _f = f;
            Name = prop;
        }

        public string Run(T? obj)
        {
            if (null == obj) return "";
            var ret = _f(obj);
            return ret;
        }
    }

    private readonly List<Extractor> _valueSelectors = new();
    public HashAlgorithm HashAlgorithm { get; set; } = new HMACSHA256();

    #region Adds

    public FingerprintBuilder<T> Add(Expression<Func<T, int>> me) => AddToString(me);
    public FingerprintBuilder<T> Add(Expression<Func<T, long>> me) => AddToString(me);
    public FingerprintBuilder<T> AddNullable(Expression<Func<T, long?>> me) => AddToString(me);

    public FingerprintBuilder<T> Add_Nullable<TVal>(Expression<Func<T, TVal?>> me, Func<TVal?, string> f) =>
        Add(me, o => o == null ? "" : f(o));

    public FingerprintBuilder<T> Add(Expression<Func<T, float>> me) =>
        Add(me, f => f.ToString(CultureInfo.InvariantCulture));

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, float?>> me) =>
        Add(me, f => f?.ToString(CultureInfo.InvariantCulture) ?? "");

    public FingerprintBuilder<T> Add<T2>(Expression<Func<T, T2>> me, Fingerprinter<T2> fp) =>
        Add(me, fp.Get);

    public FingerprintBuilder<T> Add(Expression<Func<T, double>> me) =>
        Add(me, f => f.ToString(CultureInfo.InvariantCulture));

    public FingerprintBuilder<T> Add(Expression<Func<T, DateTime>> me) =>
        Add(me, f => f.ToSaneString());

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, DateTime?>> me) =>
        Add(me, f => f.ToSaneString());

    public FingerprintBuilder<T> Add(Expression<Func<T, DateTime?>> me) =>
        Add(me, f => f.ToSaneString());

    public FingerprintBuilder<T> Add(Expression<Func<T, byte[]>> me) =>
        Add(me, f => f.ToHexString());

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, byte[]?>> me) =>
        Add(me, f => f.ToHexString());

    public FingerprintBuilder<T> Add(Expression<Func<T, DateOnly>> me) =>
        Add(me, f => f.ToSaneString());

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, DateOnly?>> me) =>
        Add(me, f => f?.ToSaneString() ?? "");

    public FingerprintBuilder<T> Add(Expression<Func<T, TimeOnly>> me) =>
        Add(me, f => f.ToSaneString());

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, TimeOnly?>> me) =>
        Add(me, f => f?.ToSaneString() ?? "");

    public FingerprintBuilder<T> Add(Expression<Func<T, string>> me) =>
        Add(me, s => s);

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, string?>> me) =>
        Add(me, s => s ?? "");

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, int?>> me) =>
        AddToString(me);

    public FingerprintBuilder<T> Add(Expression<Func<T, bool>> me) =>
        Add(me, s => s ? "true" : "false");

    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, bool?>> me) =>
        Add(me, s => s switch
        {
            null => "",
            true => "true",
            false => "false"
        });

    public FingerprintBuilder<T> Add(Expression<Func<T, Guid>> me) => AddToString(me);
    public FingerprintBuilder<T> Add_Nullable(Expression<Func<T, Guid?>> me) => AddToString(me);

    private FingerprintBuilder<T> AddToString<TObj>(Expression<Func<T, TObj>> me) =>
        Add(me, o => o?.ToString() ?? "");

    public FingerprintBuilder<T> Add<TInput>(Expression<Func<T, TInput>> getMemberExpression,
        Func<TInput, string> transform)
    {
        if (getMemberExpression == null) throw new ArgumentNullException(nameof(getMemberExpression));
        if (getMemberExpression.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Expression must be a member expression.");
        var name = memberExpression.Member.Name;
        if (_valueSelectors.Any(e => e.Name.EqualsOic(name)))
            throw new ArgumentException($"Member '{name}' has already been added.");

        var getValue = getMemberExpression.Compile();

        _valueSelectors.Add(new(name, o => transform(getValue(o))));

        return this;
    }

    #endregion

    public FingerprintBuilder<T> With(HashAlgorithm ha)
    {
        HashAlgorithm = ha;
        return this;
    }

    public Fingerprinter<T> Build()
    {
        if (_valueSelectors.Count != 0 == false)
            throw new InvalidOperationException("You need to specify at least one selector.");

        return new Fingerprinter<T>(obj =>
                _valueSelectors
                    .Select(item => item.Run(obj))
                    .NotNull()
                    .ComputeHash(HashAlgorithm)
                    .ToHexString(),
            HashAlgorithm);
    }
}

public interface IFingerPrinter;

public class Fingerprinter<T> : IFingerPrinter
{
    private readonly Func<T?, string> _single;
    private readonly Func<IEnumerable<T>?, string> _multi;

    public Fingerprinter(Func<T?, string> single, HashAlgorithm hashAlgorithm)
    {
        _single = o => null == o ? "" : single(o);
        _multi = os => os?
            .Select(_single)
            .ComputeHash(hashAlgorithm)
            .ToHexString() ?? "";
    }

    public string Get(T? obj) => _single(obj);
    public string Get(IEnumerable<T>? objs) => _multi(objs);
}

public static class FingerprintBuilder
{
    public static FingerprintBuilder<T> For<T>() => new FingerprintBuilder<T>();

    public static FingerprintBuilder<T> For<T>(T obj) => For<T>();

    public static byte[] ComputeHash(this IEnumerable<string> strs, HashAlgorithm ha) => ha.ComputeHash(strs);

    public static byte[] ComputeHash(this HashAlgorithm ha, IEnumerable<string> strs)
    {
        using var memory = new MemoryStream();
        strs
            .NotNull()
            .Select(Encoding.UTF8.GetBytes)
            .Aggregate(memory, (stream, bs) => stream.Write(bs));
        memory.Position = 0;
        return ha.ComputeHash(memory);
    }
}