using System;

namespace Morpheus.UI
{
    public enum UiAnimationClip
    {
        Idle = 0,
        Show = 1,
        Hide = ~Show,
    }

    public interface IUiAnimation
    {
        void Initialize(Action<object> completeCb);
        bool Has(UiAnimationClip clip, string suffix);
        void Play(UiAnimationClip clip, string suffix);
        void Stop();
    }
}