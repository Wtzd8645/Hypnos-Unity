using System.Collections.Generic;

namespace Blanketmen.Hypnos
{
    public class AnimationManager
    {
        #region Singleton
        public static AnimationManager Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance ??= new AnimationManager();
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }

        private AnimationManager() { }
        #endregion

        private AnimationLayerBase[] layers;
        private Dictionary<int, IAnimator> animators = new Dictionary<int, IAnimator>(107);

        public void FixedUpdate(float deltaTime)
        {
            foreach (IAnimator animator in animators.Values)
            {
                animator.FixedUpdate(deltaTime);
            }
        }

        public void Update(float deltaTime)
        {
            foreach (IAnimator animator in animators.Values)
            {
                animator.Update(deltaTime);
            }
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (IAnimator animator in animators.Values)
            {
                animator.LateUpdate(deltaTime);
            }
        }
    }
}