using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blanketmen.Hypnos.UI
{
    public class UiButton : Button
    {
        [SerializeField] protected int objectId;
        [SerializeField] protected string onClickSoundId;

        public override void OnPointerClick(PointerEventData eventData)
        {
            PlaySound();
            base.OnPointerClick(eventData);
        }

        protected void PlaySound()
        {
            // TODO: Implement.
        }
    }
}