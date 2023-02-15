using Hypnos.GameData;
using TMPro;
using UnityEngine;

namespace Hypnos.UI
{
    // NOTE: An 2048x2048 atlas can contain about 5350 characters with 24pt.
    public class UiText : TextMeshProUGUI
    {
        [SerializeField] protected int styleId;
        [SerializeField] protected uint i18nTextId;

#if UNITY_EDITOR // NOTE: For #define of the parent class.
        protected override void Reset()
        {
            base.Reset();
            raycastTarget = false;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            if (i18nTextId != 0u)
            {
                SetText(I18nTextManager.Instance.GetText(i18nTextId));
            }
        }

        public void SetI18nText(uint id, params string[] args)
        {
            if (args == null)
            {
                SetText(I18nTextManager.Instance.GetText(id));
            }
            else
            {
                SetText(string.Format(I18nTextManager.Instance.GetText(id), args));
            }
        }
    }
}