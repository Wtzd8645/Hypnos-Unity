using UnityEngine;

namespace Blanketmen.Hypnos.UI
{
    public class UiTweenAnimation //: DOTweenAnimation, IUiAnimation
    {
        [SerializeField] protected UiAnimationClip clipType;
        /*
        protected void Reset()
        {
            autoPlay = false;
            autoKill = false;
        }

        void IUiAnimation.SetCompleteCallback(Action callback)
        {
            tween.onRewind = new TweenCallback(callback);
            tween.onComplete = new TweenCallback(callback);
        }

        bool IUiAnimation.Play(UiAnimationClip clip)
        {
            if (clip == clipType)
            {
                DOTween.Restart(gameObject, id);
                return true;
            }

            if (~clip == clipType)
            {
                DOTween.PlayBackwards(gameObject, id);
                return true;
            }

            return false;
        }
        */
    }
}