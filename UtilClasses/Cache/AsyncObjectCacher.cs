using System;
using System.Threading.Tasks;

namespace UtilClasses.Cache;

public class AsyncObjectCacher<T>
{
    private Func<Task<T>> _updateFunc;
    private TimeSpan _cacheTimeout;
    private DateTime _lastUpdate = DateTime.MinValue;
    private TimeSpan? _minRefreshDelay;
    private T _obj;

    public AsyncObjectCacher(Func<Task<T>> updateFunc)
    {
        _minRefreshDelay = null;
        _cacheTimeout = TimeSpan.MaxValue;
        _updateFunc = updateFunc;
    }
    public AsyncObjectCacher(Func<Task<T>> updateFunc, TimeSpan cacheTimeout, TimeSpan? minRefreshDelay=null)
    {
        _minRefreshDelay = minRefreshDelay;
        _cacheTimeout = cacheTimeout;
        _updateFunc = updateFunc;
    }
    protected bool UpdateNeeded()
    {
        if (_lastUpdate == DateTime.MinValue) return true;
        if (_cacheTimeout.TotalMilliseconds == 0) return true;
        if (_cacheTimeout == TimeSpan.MaxValue) return false;
        if (_minRefreshDelay.HasValue && _lastUpdate + _minRefreshDelay.Value < DateTime.Now) return false;
        return DateTime.Now > _lastUpdate + _cacheTimeout;
    }

    public virtual void Invalidate()
    {
        _lastUpdate = DateTime.MinValue;
    }

    public async Task Update()
    {
        _obj = await _updateFunc();
    }

    public async Task<T> Get()
    {
        await UpdateIfNeeded();
        return _obj;
    }

    private async Task UpdateIfNeeded()
    {
        if (UpdateNeeded()) await Update();
    }

}