using Blanketmen.Hypnos.Compression;
using Blanketmen.Hypnos.Encryption;
using Blanketmen.Hypnos.Serialization;
using System;
using System.IO;

namespace Blanketmen.Hypnos
{
    public class DataArchiver
    {
        private readonly ISerializer serializer;
        private readonly ICompressor compressor;
        private readonly IEncryptor encryptor;

        public DataArchiver(ISerializer serializer, ICompressor compressor = null, IEncryptor encryptor = null)
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
                using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                fileStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public T Load<T>(string filePath)
        {
            try
            {
                byte[] data;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int offset = 0;
                    if (fileStream.Length > int.MaxValue)
                    {
                        throw new IOException("File length exceeds 2GB");
                    }

                    int unreadLength = (int)fileStream.Length;
                    data = new byte[unreadLength];
                    while (unreadLength > 0)
                    {
                        int readLength = fileStream.Read(data, offset, unreadLength);
                        if (readLength == 0)
                        {
                            throw new EndOfStreamException("Read beyond EOF");
                        }

                        offset += readLength;
                        unreadLength -= readLength;
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
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}