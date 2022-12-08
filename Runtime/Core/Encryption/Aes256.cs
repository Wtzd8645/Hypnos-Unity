using System;
using System.Security.Cryptography;

namespace Morpheus.Core.Encryption
{
    public class Aes256 : IEncryptor
    {
        private readonly Aes encryption = Aes.Create();

        public byte[] Key
        {
            get => encryption.Key;
            set => encryption.Key = value;
        }

        public byte[] Iv
        {
            get => encryption.IV;
            set => encryption.IV = value;
        }

        public Aes256()
        {
            encryption.GenerateKey();
            encryption.GenerateIV();
        }

        public Aes256(byte[] key, byte[] iv)
        {
            if (key == null || key.Length != 32)
            {
                throw new ArgumentException("The length of the key must be 32 bytes.");
            }
            encryption.Key = key;

            if (iv == null || iv.Length != 16)
            {
                throw new ArgumentException("The length of the initialization vector must be 16 bytes.");
            }
            encryption.IV = iv;
        }

        public byte[] GenerateKey()
        {
            encryption.GenerateKey();
            return encryption.Key;
        }

        public byte[] GenerateIv()
        {
            encryption.GenerateIV();
            return encryption.IV;
        }

        public int Encrypt(byte[] src, int srcOffset, int count, byte[] dst, int dstOffset)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] src, int offset, int count)
        {
            return encryption.CreateEncryptor().TransformFinalBlock(src, offset, count);
        }

        public int Decrypt(byte[] src, int srcOffset, int count, byte[] dst, int dstOffset)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] src, int offset, int count)
        {
            return encryption.CreateDecryptor().TransformFinalBlock(src, offset, count);
        }
    }
}