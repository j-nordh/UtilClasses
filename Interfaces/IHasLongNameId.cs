namespace UtilClasses.Interfaces;

public interface IHasLongNameId : IHasLongId, IHasName
{
}

public class LongNameId : IHasLongNameId
{
    public string Name { get; set; }

    public long Id { get; set; }
}