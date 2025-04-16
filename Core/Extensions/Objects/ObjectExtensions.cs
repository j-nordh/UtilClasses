using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using UtilClasses.Core.Extensions.Dictionaries;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.Objects;

[PublicAPI]
public static class ObjectExtensions
{
    private static readonly Dictionary<Type, Func<IConvertible, object>> _converters;

    static ObjectExtensions()
    {
        _converters = new Dictionary<Type, Func<IConvertible, object>>
        {
            { typeof(bool), o => o.ToString(CultureInfo.CurrentCulture).AsBoolean() },
            { typeof(char), o => o.ToChar(CultureInfo.CurrentCulture) },
            { typeof(byte), o => o.ToByte(CultureInfo.CurrentCulture) },
            { typeof(int), o => o.ToInt32(CultureInfo.CurrentCulture) },
            { typeof(long), o => o.ToInt64(CultureInfo.CurrentCulture) },
            { typeof(double), o => o.ToDouble(CultureInfo.CurrentCulture) },
            { typeof(DateTime), o => o.ToDateTime(CultureInfo.CurrentCulture) },
            { typeof(string), o => o.ToString(CultureInfo.CurrentCulture) },
            { typeof(float), o => o.ToSingle(CultureInfo.CurrentCulture) }
        };
    }

    public static T With<T>(this T obj, Action<T> a)
    {
        a(obj);
        return obj;
    }

    public static T ThrowIfNull<T>(this T obj, Func<Exception> f)
    {
        if (null == obj) throw f();
        return obj;
    }

    public static T ThrowIfNull<T>(this T obj, Exception e) => obj.ThrowIfNull(() => e);

    public static T ThrowIfNull<T>(this T obj, string message) =>
        obj.ThrowIfNull(() => new Exception(message));


    public static void As<T>(this object obj, Action<T> action) where T : class
    {
        if (obj is T o) action(o);
    }

    public static T? As<T>(this object obj) where T : class
    {
        return obj as T;
    }

    public static T HardCast<T>(this object obj) => (T)obj;


    [Obsolete("Use WhenNotNull instead")]
    public static TOut? IfNotNull<TIn, TOut>(this TIn? obj, Func<TIn, TOut> f) where TIn : class where TOut : class
        => null == obj ? null : f(obj);

    [Obsolete("Use WhenNotNull instead")]
    public static TOut? IfNotNull<TIn, TOut>(this TIn? obj, Func<TOut> f) where TIn : class where TOut : class
        => null == obj ? null : f();

    public static TOut? WhenNotNull<TIn, TOut>(this TIn? obj, Func<TIn, TOut> f)
        where TIn : class where TOut : class
        => null == obj ? null : f(obj);

    public static TOut? WhenNotNull<TIn, TOut>(this TIn? obj, Func<TOut> f) where TIn : class where TOut : class
        => null == obj ? null : f();

    public static TOut? WhenNull<TIn, TOut>(this TIn? obj, Func<TOut> fNull, Func<TOut> fNotNull)
        where TIn : class where TOut : class
        => null == obj ? fNull() : fNotNull();

    public static TOut WhenNull<TIn, TOut>(this TIn obj, Func<TOut> nullFunc, Func<TIn, TOut> notNullFunc)
        where TIn : class => obj.When(o => null == o, nullFunc, () => notNullFunc(obj));

    public static void WhenNotNull<TIn>(this TIn? obj, Action<TIn> action) where TIn : class
    {
        if (null != obj)
            action(obj);
    }

    public static void WhenNotNull<T>(this T? obj, Action<T> action) where T : struct
    {
        if (!obj.HasValue) return;
        action(obj.Value);
    }

    public static TOut When<TIn, TOut>(this TIn obj, bool predicate, Func<TOut> onTrue, Func<TOut> onFalse) =>
        obj.When(_ => predicate, onTrue, onFalse);

    public static TOut When<TIn, TOut>(this TIn obj, Func<TIn, bool> predicate, Func<TOut> onTrue,
        Func<TOut> onFalse)
        => predicate(obj) ? onTrue() : onFalse();

    public static TOut When<TIn, TOut>(this TIn obj, Func<TIn, bool> predicate, TOut onTrue, TOut onFalse)
        => obj.When(predicate, () => onTrue, () => onFalse);

    public static TOut When<TIn, TOut>(this TIn obj, bool predicate, TOut onTrue, TOut onFalse)
        => obj.When(predicate, () => onTrue, () => onFalse);

    public static void Try<T>(this T o, Action<T> action, Action<Exception> onException)
    {
        try
        {
            action(o);
        }
        catch (Exception e)
        {
            onException.Invoke(e);
        }
    }

    public static T Convert<T>(this object obj) where T : struct, IConvertible
    {
        if (obj is T o) return o;
        if (obj is not IConvertible convertible) throw new InvalidCastException();

        var conv = _converters.Maybe(typeof(T));
        if (null == conv)
            throw new InvalidCastException();
        return (T)conv(convertible);
    }

    public static bool AsBoolean(this object obj) => obj as bool? ?? StringExtensions.AsBoolean(obj.ToString());

    public static byte? MaybeAsByte(this object o) => StringExtensions.MaybeAsByte(o.ToString());
    public static byte? MaybeAsByte(this object o, bool predicate) => predicate ? o.MaybeAsByte() : null;


    public static T Do<T>(this T obj, Action<T> a)
    {
        a(obj);
        return obj;
    }

    public static T Do<T>(this T obj, Action a)
    {
        a();
        return obj;
    }

    public static T Do<T>(this T obj, bool predicate, Action a) => predicate ? obj.Do(a) : obj;


    public static T AsLock<T>(this object o, Func<T> f)
    {
        lock (o)
            return f();
    }

    public static void AsLock<T>(this T o, Action<T> a)
    {
        if (null == o) throw new ArgumentNullException(nameof(o));
        lock (o)
            a(o);
    }

    public static TOut AsLock<TIn, TOut>(this TIn o, Func<TIn, TOut> f)
    {
        if (null == o) throw new ArgumentNullException(nameof(o));
        lock (o)
            return f(o);
    }

    public static Dictionary<string, string> AsDictionary(this object o, StringComparer? comp = null) =>
        TypeDescriptor.GetProperties(o)
            .Cast<PropertyDescriptor>()
            .ToDictionary(p => p.Name, p => p.GetValue(o)?.ToString() ?? "",
                comp ?? StringComparer.OrdinalIgnoreCase);
}