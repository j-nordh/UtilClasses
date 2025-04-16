using System;

namespace UtilClasses.Core.Cache;

public class ObjectCacher<T>: Cacher where T:class
{
    private readonly Func<T> _updateFunc;
    private T? _obj;
    public ObjectCacher(Func<T> updateFunc, TimeSpan cacheTimeout, TimeSpan? minRefreshDelay) : base(cacheTimeout, minRefreshDelay)
    {
        _updateFunc = updateFunc;
        _obj = null;
    }

    public void Update()
    {
        _obj = _updateFunc();
    }

    public T? Get()
    {
        UpdateIfNeeded();
        return _obj;
    }

    private void UpdateIfNeeded()
    {
        if (UpdateNeeded()) Update();
    }
    public static implicit operator T? (ObjectCacher<T> c)=>c.Get();
}