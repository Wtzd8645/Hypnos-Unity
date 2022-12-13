using UnityEngine;
using UnityEngine.UI;

namespace Morpheus.UI
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class InteractableUiBase : UiBase
    {
        [SerializeField] protected GraphicRaycaster graphicRaycaster;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }
#endif

        protected internal override void OnCreate()
        {
            gameObject.SetActive(true);
            SetInteractable(false);
            SetAnimations();
            SetVisible(false);
            DisplayState = UiDisplayState.Hide;
            Initialize();
        }

        public virtual void SetInteractable(bool isInteractable)
        {
            graphicRaycaster.enabled = isInteractable;
            // TODO: Joystick input.
        }

        public override void Show(UiAoHandler showCompleteCb = null)
        {
            if (DisplayState != UiDisplayState.Hide)
            {
                Kernel.LogWarning($"[InteractableUiBase] Show UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
                showCompleteCb?.Invoke(this);
                return;
            }

            if (isTransiting)
            {
                TerminateTransition();
            }

            DisplayState = UiDisplayState.Show;
            onTransitionComplete = showCompleteCb;
            SetInteractable(false);
            OnShow();
            SetVisible(true);
            Transit(UiAnimationClip.Show);
        }

        public override void ShowImmediate()
        {
            if (DisplayState != UiDisplayState.Hide)
            {
                Kernel.LogWarning($"[InteractableUiBase] Show UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
                return;
            }

            if (isTransiting)
            {
                TerminateTransition();
            }

            DisplayState = UiDisplayState.Show;
            OnShow();
            SetVisible(true);
            OnShowComplete();
            SetInteractable(true);
        }

        public override void Hide(UiAoHandler hideCompleteCb = null)
        {
            if (DisplayState != UiDisplayState.Show)
            {
                Kernel.LogWarning($"[InteractableUiBase] Hide UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
                hideCompleteCb?.Invoke(this);
                return;
            }

            if (isTransiting)
            {
                TerminateTransition();
            }

            DisplayState = UiDisplayState.Hide;
            onTransitionComplete = hideCompleteCb;
            SetInteractable(false);
            OnHide();
            Transit(UiAnimationClip.Hide);
        }

        public override void HideImmediate()
        {
            if (DisplayState != UiDisplayState.Show)
            {
                Kernel.LogWarning($"[InteractableUiBase] Hide UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
                return;
            }

            if (isTransiting)
            {
                TerminateTransition();
            }

            DisplayState = UiDisplayState.Hide;
            SetInteractable(false);
            OnHide();
            SetVisible(false);
            OnHideComplete();
        }

        protected override void OnTransitionComplete()
        {
            switch (DisplayState)
            {
                case UiDisplayState.Show:
                {
                    OnShowComplete();
                    SetInteractable(true);
                    break;
                }
                case UiDisplayState.Hide:
                {
                    SetVisible(false);
                    OnHideComplete();
                    break;
                }
            }

            if (onTransitionComplete != null)
            {
                onTransitionComplete(this);
                onTransitionComplete = null;
            }
        }
    }
}