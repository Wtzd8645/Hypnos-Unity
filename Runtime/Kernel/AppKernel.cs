using Morpheus.Environment;
using Morpheus.GameState;
using Morpheus.GameTime;
using Morpheus.IO;
using Morpheus.Network;
using Morpheus.Resource;
using System;

namespace Morpheus
{
    public sealed partial class AppKernel
    {
        public static AppKernel Instance { get; private set; }

        public event Action<bool> OnApplicationPauseEvent;
        public event Action OnApplicationQuitEvent;

        private void CreateServices()
        {
            EnvironmentManager.CreateInstance();
            ResourceManager.CreateInstance();
            NetworkManager.CreateInstance();
            GameTimeManager.CreateInstance();
            GameStateManager.CreateInstance();
            InputManager.CreateInstance();
        }

        private void ReleaseServices()
        {
            EnvironmentManager.ReleaseInstance();
            ResourceManager.ReleaseInstance();
            NetworkManager.ReleaseInstance();
            GameTimeManager.ReleaseInstance();
            GameStateManager.ReleaseInstance();
            InputManager.ReleaseInstance();
        }
    }
}