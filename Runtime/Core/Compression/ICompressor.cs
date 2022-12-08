namespace Morpheus.Core.Compression
{
    public interface ICompressor
    {
        byte[] Compress(byte[] src, int offset, int count);
        byte[] Decompress(byte[] src, int offset, int count);

        void Compress(byte[] src, int srcOffset, int srcCount, byte[] dest, int destOffset, int destCount);
        void Decompress(byte[] src, int srcOffset, int srcCount, byte[] dest, int destOffset, int destCount);
    }
}