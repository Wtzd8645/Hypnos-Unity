namespace Blanketmen.Hypnos
{
    internal class AssetInst
    {
        public string assetId;
        public string groupId;

        public UnityEngine.Object asset;
        public int refCount;

        public bool Dereference()
        {
            if (--refCount > 0)
            {
                return false;
            }

            if (refCount < 0)
            {
                Logging.Error($"Reference count is abnormal. AssetId: {assetId}", nameof(AssetInst));
                asset = null;
                refCount = 0;
                return false;
            }

            asset = null;
            return true;
        }
    }
}