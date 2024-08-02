using System.Collections.Generic;
using UnityEngine;

namespace Blanketmen.Hypnos.UI
{
    public enum UiDisplayState
    {
        None,
        Show,
        Hide
    }

    /// <summary>
    /// Only one prefab of a specific type inheriting from this base class can be loaded at a time.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public abstract class UiBase : MonoBehaviour
    {
        public const string IdleClipName = "Idle";
        public const string ShowClipName = "Show";
        public const string HideClipName = "Hide";
        public const string OnUiAnimationCompleteFunctionName = "OnUiAnimationComplete";

        internal UiBase nextNode;
        internal float elapsedTimeAfterClosed;

        protected string uiAnimationSuffix = string.Empty; // NOTE: 提供給有多種動畫時使用
        protected IUiAnimation[] animations;
        protected HashSet<IUiAnimation> playingAnimations;
        protected int playingAnimationCount;

        protected bool isTransiting;
        protected UiAoHandler onTransitionComplete;

        public UiDisplayState DisplayState { get; protected set; }
        public bool IsOpen
        {
            get { return elapsedTimeAfterClosed < 0f; }
            set { elapsedTimeAfterClosed = value ? float.MinValue : 0f; }
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            gameObject.layer = UiManager.VisibleUiLayer;
        }
#endif

        public virtual void SetVisible(bool visible)
        {
            gameObject.layer = visible ? UiManager.VisibleUiLayer : UiManager.InvisibleUiLayer;
        }

        protected internal virtual void OnCreate()
        {
            SetAnimations();
            SetVisible(false);
            DisplayState = UiDisplayState.Hide;
            Initialize();
        }

        protected internal virtual void OnRuin()
        {
            switch (DisplayState)
            {
                case UiDisplayState.Show:
                {
                    if (isTransiting)
                    {
                        TerminateTransition();
                        OnShowComplete();
                    }

                    if (elapsedTimeAfterClosed < 0f)
                    {
                        OnClose();
                    }

                    OnHide();
                    OnHideComplete();
                    break;
                }
                case UiDisplayState.Hide:
                {
                    if (isTransiting)
                    {
                        TerminateTransition();
                        OnHideComplete();
                    }

                    if (elapsedTimeAfterClosed < 0f)
                    {
                        OnClose();
                    }
                    break;
                }
                default:
                {
                    return;
                }
            }

            Release();
        }

        public abstract void Initialize();

        public abstract void Release();

        public virtual void Refresh() { }

        public virtual void OnOpen() { elapsedTimeAfterClosed = float.MinValue; }

        public virtual void Close() { UiManager.Instance.OperateAsync(GetType(), UiManager.CloseOperation); }

        public virtual void OnClose() { elapsedTimeAfterClosed = 0f; }

        public virtual void Show(UiAoHandler showCompleteCb = null)
        {
            if (DisplayState != UiDisplayState.Hide)
            {
                Logging.Warning($"[UiBase] Show UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
                showCompleteCb?.Invoke(this);
                return;
            }

            if (isTransiting)
            {
                TerminateTransition();
            }

            DisplayState = UiDisplayState.Show;
            onTransitionComplete = showCompleteCb;
            OnShow();
            SetVisible(true);
            Transit(UiAnimationClip.Show);
        }

        public virtual void ShowImmediate()
        {
            if (DisplayState != UiDisplayState.Hide)
            {
                Logging.Warning($"[UiBase] Show UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
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
        }

        protected virtual void OnShow() { }

        protected virtual void OnShowComplete() { }

        public virtual void Hide(UiAoHandler hideCompleteCb = null)
        {
            if (DisplayState != UiDisplayState.Show)
            {
                Logging.Warning($"[UiBase] Hide UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
                hideCompleteCb?.Invoke(this);
                return;
            }

            if (isTransiting)
            {
                TerminateTransition();
            }

            DisplayState = UiDisplayState.Hide;
            onTransitionComplete = hideCompleteCb;
            OnHide();
            Transit(UiAnimationClip.Hide);
        }

        public virtual void HideImmediate()
        {
            if (DisplayState != UiDisplayState.Show)
            {
                Logging.Warning($"[UiBase] Hide UI at wrong state. Name: {gameObject.name}, State: {DisplayState}");
                return;
            }

            if (isTransiting)
            {
                TerminateTransition();
            }

            DisplayState = UiDisplayState.Hide;
            OnHide();
            SetVisible(false);
            OnHideComplete();
        }

        protected virtual void OnHide() { }

        protected virtual void OnHideComplete() { }

        protected void SetAnimations()
        {
            animations = GetComponentsInChildren<IUiAnimation>();
            for (int i = 0; i < animations.Length; ++i)
            {
                animations[i].Initialize(OnAnimationComplete);
            }
            playingAnimations = new HashSet<IUiAnimation>();
        }

        protected void Transit(UiAnimationClip animationClip)
        {
            isTransiting = true;
            for (int i = 0; i < animations.Length; ++i)
            {
                if (animations[i].Has(animationClip, uiAnimationSuffix))
                {
                    playingAnimations.Add(animations[i]);
                }
            }

            if (playingAnimations.Count == 0)
            {
                OnTransitionComplete();
                return;
            }

            playingAnimationCount = playingAnimations.Count;
            foreach (IUiAnimation anima in playingAnimations)
            {
                anima.Play(animationClip, uiAnimationSuffix);
            }
        }

        public void TerminateTransition()
        {
            onTransitionComplete = null;
            foreach (IUiAnimation anima in playingAnimations)
            {
                anima.Stop();
            }
            playingAnimations.Clear();
            playingAnimationCount = 0;
            isTransiting = false;
        }

        protected void OnAnimationComplete(object animation)
        {
            if (--playingAnimationCount > 0)
            {
                return;
            }

            playingAnimations.Clear();
            OnTransitionComplete();
        }

        // NOTE: For UnityEngine.Animaion
        protected void OnUiAnimationComplete(AnimationEvent animationEvent)
        {
            OnAnimationComplete(animationEvent);
        }

        protected virtual void OnTransitionComplete()
        {
            isTransiting = false;
            switch (DisplayState)
            {
                case UiDisplayState.Show:
                {
                    OnShowComplete();
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