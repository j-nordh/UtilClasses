using System;

namespace UtilClasses.Interfaces
{
    public interface IHasId<T>
    {
        T Id { get; }
    }
    public interface IHasWriteId<T> {
        T Id { get; set; }
    }
    public interface IHasLongId:IHasId<long>;
    public interface IHasIntId: IHasId<int>;
    public interface IHasWriteIntId: IHasWriteId<int>;
    public interface IHasGuid : IHasId<Guid>;

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

    public interface IHasNameIntId : IHasIntId, IHasName
    {
    }
    

    public interface IHasReadOnlyNameGuid
    {
        Guid Id { get; }
        string Name { get; }
    }
}