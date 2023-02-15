using System;

namespace Hypnos.UI
{
    public class BindingResourceAttribute : Attribute
    {
        public string AssetId { get; private set; }
        public int RootId { get; private set; }
        public bool IsDontDestroy { get; private set; }

        public BindingResourceAttribute(string assetId, int rootId, bool isDontDestroy = false)
        {
            AssetId = assetId;
            RootId = rootId;
            IsDontDestroy = isDontDestroy;
        }
    }
}