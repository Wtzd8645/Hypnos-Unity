using Blanketmen.Hypnos.Compression;
using Blanketmen.Hypnos.Encryption;
using Blanketmen.Hypnos.Serialization;

namespace Blanketmen.Hypnos
{
    public enum CoreSerializer
    {
        None,
        Json,
        DotNetXml,
        DotNetBinary,
    }

    public enum CoreCompressor
    {
        None
    }

    public enum CoreEncryptor
    {
        None
    }

    public static class CoreUtil
    {
        public static ISerializer CreateSerializer(CoreSerializer type)
        {
            switch (type)
            {
                case CoreSerializer.None:
                {
                    return null;
                }
                case CoreSerializer.Json:
                {
                    return new JsonSerializer();
                }
                case CoreSerializer.DotNetXml:
                {
                    return new DotNetXmlSerializer();
                }
                case CoreSerializer.DotNetBinary:
                {
                    return new DotNetBinaryFormatter();
                }
                default:
                {
                    Logging.Error($"[Utility] Serializer not implemented. Type: {type}");
                    return null;
                }
            }
        }

        public static ICompressor CreateCompressor(CoreCompressor type)
        {
            switch (type)
            {
                case CoreCompressor.None:
                {
                    return null;
                }
                default:
                {
                    Logging.Error($"[Utility] Compressor not implemented. Type: {type}");
                    return null;
                }
            }
        }

        public static IEncryptor CreateEncryptor(CoreEncryptor type)
        {
            switch (type)
            {
                case CoreEncryptor.None:
                {
                    return null;
                }
                default:
                {
                    Logging.Error($"[Utility] Encryptor not implemented. Type: {type}");
                    return null;
                }
            }
        }
    }
}