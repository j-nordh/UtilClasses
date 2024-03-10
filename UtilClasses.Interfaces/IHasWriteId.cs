namespace UtilClasses.Interfaces
{
    public interface IHasWriteId : IHasId
    {
        new long Id { get; set; }
    }
}
