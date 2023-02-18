using UnityEngine;

namespace Hypnos
{
    public partial class Kernel : MonoBehaviour
    {
        private static Kernel instance;

        public void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (this == instance)
            {
                instance = null;
            }
        }
    }
}