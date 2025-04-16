using System;

namespace UtilClasses.Core.Cache;

public abstract class Cacher
{
    protected DateTime _fetched;
    protected TimeSpan _cacheTimeout;
    private readonly TimeSpan? _minRefreshDelay;

    protected Cacher(TimeSpan cacheTimeout, TimeSpan? minRefreshDelay)
    {
        _cacheTimeout = cacheTimeout;
        _minRefreshDelay = minRefreshDelay;
        _fetched = DateTime.MinValue;
    }
    protected bool UpdateNeeded()
    {
        if (_fetched == DateTime.MinValue) return true;
        if (_cacheTimeout == TimeSpan.MaxValue) return false;
        if (_minRefreshDelay.HasValue && _fetched + _minRefreshDelay.Value < DateTime.Now) return false;
        return _cacheTimeout.TotalMilliseconds > 0 && DateTime.Now > _fetched + _cacheTimeout;
    }

    public virtual void Invalidate()
    {
        _fetched = DateTime.MinValue;
    }
}