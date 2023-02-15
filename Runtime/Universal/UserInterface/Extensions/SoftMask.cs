using UnityEngine;
using UnityEngine.UI;

/// Credit NemoKrad (aka Charles Humphrey) / valtain
/// Sourced from - http://www.randomchaos.co.uk/SoftAlphaUIMask.aspx
/// Updated by valtain - https://bitbucket.org/ddreaper/unity-ui-extensions/pull-requests/33
namespace Hypnos.UI
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/Effects/Extensions/SoftMask")]
    public class SoftMask : MonoBehaviour
    {
        private readonly Vector3[] worldCorners = new Vector3[4];
        private readonly Vector3[] canvasCorners = new Vector3[4];

        [Tooltip("The area that is to be used as the container.")]
        [SerializeField] private RectTransform maskArea;

        [Tooltip("Texture to be used to do the soft alpha")]
        [SerializeField] private Texture alphaMask;

        [Tooltip("At what point to apply the alpha min range 0-1")]
        [Range(0, 1)]
        [SerializeField] private float cutOff = 0;

        [Tooltip("Implement a hard blend based on the Cutoff")]
        [SerializeField] private bool hardBlend = false;

        [Tooltip("Flip the masks alpha value")]
        [SerializeField] private bool flipAlphaMask = false;

        [Tooltip("If a different Mask Scaling Rect is given, and this value is true, the area around the mask will not be clipped")]
        [SerializeField] private bool dontClipMaskScalingRect = false;

        private Material mat;

        private Canvas cachedCanvas;
        private Transform cachedCanvasTransform;

        private Vector2 maskOffset;
        private Vector2 maskScale;

        private void Start()
        {
            if (maskArea == null)
            {
                maskArea = GetComponent<RectTransform>();
            }

            Text text = GetComponent<Text>();
            if (text != null)
            {
                mat = new Material(Shader.Find("Dream/UI/SoftMask"));
                text.material = mat;
                cachedCanvas = text.canvas;
                cachedCanvasTransform = cachedCanvas.transform;
                // For some reason, having the mask control on the parent and disabled stops the mouse interacting
                // with the texture layer that is not visible.. Not needed for the Image.
                if (transform.parent.GetComponent<Mask>() == null)
                {
                    transform.parent.gameObject.AddComponent<Mask>();
                }

                transform.parent.GetComponent<Mask>().enabled = false;
                return;
            }

            Graphic graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                mat = new Material(Shader.Find("Dream/UI/SoftMask"));
                graphic.material = mat;
                cachedCanvas = graphic.canvas;
                cachedCanvasTransform = cachedCanvas.transform;
            }
        }

        private void Update()
        {
            if (cachedCanvas != null)
            {
                SetMask();
            }
        }

        private void SetMask()
        {
            Rect worldRect = GetCanvasRect();
            maskScale.Set(1.0f / worldRect.size.x, 1.0f / worldRect.size.y);
            maskOffset = -worldRect.min;
            maskOffset.Scale(maskScale);

            mat.SetTextureOffset("_AlphaMask", maskOffset);
            mat.SetTextureScale("_AlphaMask", maskScale);
            mat.SetTexture("_AlphaMask", alphaMask);

            mat.SetFloat("_HardBlend", hardBlend ? 1 : 0);
            mat.SetInt("_FlipAlphaMask", flipAlphaMask ? 1 : 0);
            mat.SetInt("_NoOuterClip", dontClipMaskScalingRect ? 1 : 0);
            mat.SetFloat("_CutOff", cutOff);
        }

        public Rect GetCanvasRect()
        {
            if (cachedCanvas == null)
            {
                return new Rect();
            }

            maskArea.GetWorldCorners(worldCorners);
            for (int i = 0; i < 4; ++i)
            {
                canvasCorners[i] = cachedCanvasTransform.InverseTransformPoint(worldCorners[i]);
            }

            return new Rect(canvasCorners[0].x, canvasCorners[0].y, canvasCorners[2].x - canvasCorners[0].x, canvasCorners[2].y - canvasCorners[0].y);
        }
    }
}