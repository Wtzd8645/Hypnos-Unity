using System;
using UnityEngine;

namespace Blanketmen.Hypnos.UI
{
    [RequireComponent(typeof(Animator))]
    public class UiAnimator : MonoBehaviour, IUiAnimation
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string clipName;

#if UNITY_EDITOR
        private void Reset()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            clipName = name;
        }
#endif

        // NOTE: Animation使用反射來呼叫callback.
        void IUiAnimation.Initialize(Action<object> completeCb)
        {
            string showClipName = $"{clipName}_{UiBase.ShowClipName}";
            string hideClipName = $"{clipName}_{UiBase.HideClipName}";
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; ++i)
            {
                if (clips[i].name.Contains(showClipName))
                {
                    SetCompleteCb(clips[i]);
                    continue;
                }

                if (clips[i].name.Contains(hideClipName))
                {
                    SetCompleteCb(clips[i]);
                    continue;
                }
            }
        }

        bool IUiAnimation.Has(UiAnimationClip clip, string suffix)
        {
            switch (clip)
            {
                case UiAnimationClip.Show:
                {
                    return animator.HasState(0, Animator.StringToHash(UiBase.ShowClipName + suffix));
                }
                case UiAnimationClip.Hide:
                {
                    return animator.HasState(0, Animator.StringToHash(UiBase.HideClipName + suffix));
                }
                default:
                {
                    return false;
                }
            }
        }

        void IUiAnimation.Play(UiAnimationClip clip, string suffix)
        {
            animator.enabled = true;
            switch (clip)
            {
                case UiAnimationClip.Show:
                {
                    animator.Play(UiBase.ShowClipName + suffix, 0, 0f);
                    return;
                }
                case UiAnimationClip.Hide:
                {
                    animator.Play(UiBase.HideClipName + suffix, 0, 0f);
                    return;
                }
                default:
                {
                    Logging.Error($"Unexpected clip: {clip.ToString() + suffix}", nameof(UiAnimator));
                    return;
                }
            }
        }

        void IUiAnimation.Stop()
        {
            animator.enabled = false;
        }

        private void SetCompleteCb(AnimationClip clip)
        {
            foreach (AnimationEvent e in clip.events)
            {
                if (e.functionName == UiBase.OnUiAnimationCompleteFunctionName)
                {
                    return;
                }
            }

            AnimationEvent evt = new AnimationEvent
            {
                time = clip.length,
                stringParameter = animator.runtimeAnimatorController.name,
                functionName = UiBase.OnUiAnimationCompleteFunctionName
            };
            clip.AddEvent(evt);
        }
    }
}