using System;

namespace UtilClasses.Interfaces
{
    public interface IHasId
    {
        long Id { get; }
    }

    public interface IHasGuid
    {
        Guid Id { get; }
    }
    public interface ICloneable<T>
    {
        T Clone();
    }
    public interface IHasName
    {
        string Name { get; }
    }
    public interface IHasTimestamp
    {
        DateTime Timestamp { get; set; }
    }
    public interface IHasNameGuid : IHasGuid, IHasName
    { }

    public interface IHasNameId : IHasId, IHasName
    { }
}
