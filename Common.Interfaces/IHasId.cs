using System;

namespace Common.Interfaces
{
    public interface IHasId
    {
        long Id { get; }
    }
    public interface IHasGuid
    {
        Guid Id { get; }
    }

    public interface IHasCount
    {
        int Count { get; }
    }
    public interface IHasWriteGuid
    {
        Guid Id { get; set; }
    }

    public interface IHasGuidSetter
    {
        void SetGuid(Guid id);
    }

    public interface IHasName
    {
        string Name { get; set; }
    }
    public interface IHasNameGuid : IHasGuid, IHasName
    {
    }

    public interface IHasReadOnlyNameGuid
    {
        Guid Id { get; }
        string Name { get; }
    }
}
