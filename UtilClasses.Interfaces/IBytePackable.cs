namespace UtilClasses.Interfaces
{
    public interface IBytePackable
    {
        int PackedSize { get; }

        void Pack(byte[] buffer, int offset);
    }
}