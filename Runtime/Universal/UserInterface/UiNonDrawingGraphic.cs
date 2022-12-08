using UnityEngine.UI;

namespace Morpheus.UI
{
    public class UiNonDrawingGraphic : MaskableGraphic
    {
        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }

        protected override void OnPopulateMesh(VertexHelper vh) { vh.Clear(); }
    }
}