using System;

namespace Morpheus.Resource
{
    [Serializable]
    public class DataArchiverConfig
    {
        public int id;
        public CoreSerializer serializer;
        public CoreCompressor compressor;
        public CoreEncryptor encryptor;
    }
}