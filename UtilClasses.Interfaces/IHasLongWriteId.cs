namespace UtilClasses.Interfaces
{
    public interface IHasLongWriteId : IHasLongId
    {
        new long Id { get; set; }
    }
}
