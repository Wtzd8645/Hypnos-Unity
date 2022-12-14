namespace Morpheus.Resource
{
    internal class AssetInst
    {
        public string AssetId;
        public string GroupId;

        public UnityEngine.Object Asset;
        public int RefCount;

        public bool Dereference()
        {
            if (--RefCount > 0)
            {
                return false;
            }

            if (RefCount < 0)
            {
                Logger.LogError($"[AssetInst] Reference count is abnormal. AssetId: {AssetId}");
                Asset = null;
                RefCount = 0;
                return false;
            }

            Asset = null;
            return true;
        }
    }
}