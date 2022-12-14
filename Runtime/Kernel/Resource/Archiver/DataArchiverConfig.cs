using System;

namespace Morpheus.Resource
{
    [Serializable]
    public class DataArchiverConfig
    {
        public int Id;
        public CoreSerializer Serializer;
        public CoreCompressor Compressor;
        public CoreEncryptor Encryptor;
    }
}