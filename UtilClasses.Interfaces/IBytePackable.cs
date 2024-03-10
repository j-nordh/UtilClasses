namespace UtilClasses.Interfaces
{
    public interface IBytePackable
    {
        void Pack(byte[] buffer, int offset);
        int PackedSize { get; }
    }    
}