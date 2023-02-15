namespace Hypnos.Core.Encryption
{
    public interface IEncryptor
    {
        int Encrypt(byte[] src, int srcOffset, int count, byte[] dst, int dstOffset);
        byte[] Encrypt(byte[] src, int offset, int count);
        int Decrypt(byte[] src, int srcOffset, int count, byte[] dst, int dstOffset);
        byte[] Decrypt(byte[] src, int offset, int count);
    }
}