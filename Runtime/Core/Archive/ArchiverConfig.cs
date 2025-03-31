using Blanketmen.Hypnos.Compression;
using Blanketmen.Hypnos.Encryption;
using Blanketmen.Hypnos.Serialization;
using System;

namespace Blanketmen.Hypnos
{
    [Serializable]
    public class ArchiverConfig
    {
        public int id;
        public ISerializer serializer;
        public ICompressor compressor;
        public IEncryptor encryptor;
    }
}