using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Blanketmen.Hypnos.UI
{
    public class InverseMaskableImage : Image
    {
        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            Material toUse = baseMaterial;

            if (m_ShouldRecalculateStencil)
            {
                Transform rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
                m_StencilValue = maskable ? MaskUtilities.GetStencilDepth(transform, rootCanvas) : 0;
                m_ShouldRecalculateStencil = false;
            }

            // if we have a enabled Mask component then it will
            // generate the mask material. This is an optimisation
            // it adds some coupling between components though :(
            Mask maskComponent = GetComponent<Mask>();
            if (m_StencilValue > 0 && (maskComponent == null || !maskComponent.IsActive()))
            {
                Material maskMat = StencilMaterial.Add(
                    toUse,
                    (1 << m_StencilValue) - 1,
                    StencilOp.Keep,
                    CompareFunction.NotEqual,
                    ColorWriteMask.All,
                    (1 << m_StencilValue) - 1, 0);
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = maskMat;
                toUse = m_MaskMaterial;
            }
            return toUse;
        }
    }
}