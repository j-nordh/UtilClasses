using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
    private readonly SortedDictionary<string, Func<T, string>> _valueSelectors = new(StringComparer.OrdinalIgnoreCase);
    public HashAlgorithm HashAlgorithm { get; set; } = new HMACSHA256();

    public FingerprintBuilder<T> Add(Expression<Func<T, int>> me) => AddToString(me);
    public FingerprintBuilder<T> Add(Expression<Func<T, long>> me) => AddToString(me);

    public FingerprintBuilder<T> Add(Expression<Func<T, float>> me) =>
        Add(me, f => f.ToString(CultureInfo.InvariantCulture));
    public FingerprintBuilder<T> Add<T2>(Expression<Func<T, T2>> me, Fingerprinter<T2> fp) =>
        Add(me, o=>fp.Get(o));
    public FingerprintBuilder<T> Add(Expression<Func<T, double>> me) =>
        Add(me, f => f.ToString(CultureInfo.InvariantCulture));

    public FingerprintBuilder<T> Add(Expression<Func<T, DateTime>> me) =>
        Add(me, f => f.ToSaneString());
    public FingerprintBuilder<T> Add(Expression<Func<T, DateOnly>> me) =>
        Add(me, f => f.ToSaneString());
    public FingerprintBuilder<T> Add(Expression<Func<T, TimeOnly>> me) =>
        Add(me, f => f.ToSaneString());

    public FingerprintBuilder<T> Add(Expression<Func<T, string>> me) =>
        Add(me, s => s);

    public FingerprintBuilder<T> Add(Expression<Func<T, Guid>> me) => AddToString(me);

    private FingerprintBuilder<T> AddToString<TObj>(Expression<Func<T, TObj>> me) =>
        Add(me, o => o.ToString());

    public FingerprintBuilder<T> Add<TInput>(Expression<Func<T, TInput>> getMemberExpression,
        Expression<Func<TInput, string>> transformValueExpression)
    {
        if (getMemberExpression == null) throw new ArgumentNullException(nameof(getMemberExpression));
        if (transformValueExpression == null) throw new ArgumentNullException(nameof(transformValueExpression));
        if (getMemberExpression.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Expression must be a member expression.");
        if (_valueSelectors.ContainsKey(memberExpression.Member.Name))
            throw new ArgumentException($"Member '{memberExpression.Member.Name}' has already been added.");

        var getValue = getMemberExpression.Compile();
        var transform = transformValueExpression.Compile();

        _valueSelectors[memberExpression.Member.Name] = obj => transform(getValue(obj));

        return this;
    }

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
                .Select(item => item.Value(obj))
                .NotNull()
                .ComputeHash(HashAlgorithm)
                .ToHexString(), 
            HashAlgorithm);
    }
}

public class Fingerprinter<T>
{
    private readonly Func<T, string> _single;
    private readonly Func<IEnumerable<T>, string> _multi;

    public Fingerprinter(Func<T, string> single, HashAlgorithm hashAlgorithm)
    {
        _single = single;
        _multi = os => os
            .Select(_single)
            .ComputeHash(hashAlgorithm)
            .ToHexString();
    }

    public string Get(T obj) => _single(obj);
    public string Get(IEnumerable<T> objs) => _multi(objs);
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
        return ha.ComputeHash(memory);
    }
}