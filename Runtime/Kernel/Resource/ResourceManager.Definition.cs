namespace Hypnos.Resource
{
    public delegate void ResourceAoHandler<T>(T obj);

    public enum ResourceLoader
    {
        FastEditor, // Development
        BuildInAssetBundle, // Testing
        AssetBundle // Staging & Production
    }

    public sealed partial class ResourceManager
    {
        public const CoreSerializer AssetConfigSerializer = CoreSerializer.Json;
        public const CoreCompressor AssetConfigCompressor = CoreCompressor.None;
        public const CoreEncryptor AssetConfigEncryptor = CoreEncryptor.None;

        public const string ResourcesDirectoryName = "Resources";
        public const string AssetConfigFileName = "AssetConfig.dat";
    }
}