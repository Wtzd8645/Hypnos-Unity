using Blanketmen.Hypnos.Compression;
using Blanketmen.Hypnos.Encryption;
using Blanketmen.Hypnos.Serialization;
using System;
using System.IO;

namespace Blanketmen.Hypnos
{
    public class Archiver
    {
        private readonly ISerializer serializer;
        private readonly ICompressor compressor;
        private readonly IEncryptor encryptor;

        public Archiver(ISerializer serializer, ICompressor compressor = null, IEncryptor encryptor = null)
        {
            this.serializer = serializer;
            this.compressor = compressor;
            this.encryptor = encryptor;
        }

        public void Save<T>(T obj, string filePath)
        {
            try
            {
                byte[] data = serializer.Serialize(obj);
                if (compressor != null)
                {
                    data = compressor.Compress(data, 0, data.Length);
                }

                if (encryptor != null)
                {
                    data = encryptor.Encrypt(data, 0, data.Length);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                fs.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T Load<T>(string filePath)
        {
            try
            {
                byte[] data;
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int offset = 0;
                    if (fs.Length > int.MaxValue)
                    {
                        throw new IOException("File length exceeds 2GB");
                    }

                    int unreadLen = (int)fs.Length;
                    data = new byte[unreadLen];
                    while (unreadLen > 0)
                    {
                        int readLength = fs.Read(data, offset, unreadLen);
                        if (readLength == 0)
                        {
                            throw new EndOfStreamException("Read beyond EOF");
                        }

                        offset += readLength;
                        unreadLen -= readLength;
                    }
                }

                if (encryptor != null)
                {
                    data = encryptor.Decrypt(data, 0, data.Length);
                }

                if (compressor != null)
                {
                    data = compressor.Decompress(data, 0, data.Length);
                }

                return serializer.Deserialize<T>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}