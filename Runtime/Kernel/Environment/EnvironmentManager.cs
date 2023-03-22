using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blanketmen.Hypnos
{
    public class EnvironmentManager
    {
        #region Singleton
        public static EnvironmentManager Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance ??= new EnvironmentManager();
        }

        public static void ReleaseInstance()
        {
            Instance = null;
        }

        private EnvironmentManager() { }
        #endregion

        public string UniqueIdentifier => SystemInfo.deviceUniqueIdentifier;
        public string OperatingSystem => SystemInfo.operatingSystem;

        public int ProcessorCount => SystemInfo.processorCount;
        public int MemorySize => SystemInfo.systemMemorySize;
        public GraphicsDeviceType GraphicsApiType => SystemInfo.graphicsDeviceType;
        public int GraphicsMemorySize => SystemInfo.graphicsMemorySize;

        public string GameVersion => Application.version;
        public int TargetFrameRate => Application.targetFrameRate;

        public void Initialize(EnvironmentConfig config)
        {
            Application.runInBackground = config.runInBackground;
            Application.targetFrameRate = config.targetFrameRate;
            SetScreenResolution(config.maxScreenResolution.x, config.maxScreenResolution.y);
            Screen.sleepTimeout = config.neverSleepScreen ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting; // QUEST: 切換場景時會被重設嗎?
        }

        public void SetTargetFrameRate(int targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
        }

        // NOTE: Limit the maximum resolution of the short side.
        [Conditional("UNITY_ANDROID"), Conditional("UNITY_IOS")]
        internal void SetScreenResolution(float maxScreenResX, float maxScreenResY)
        {
            float width = Screen.width;
            float height = Screen.height;
            if (width > height && height > maxScreenResY)
            {
                float aspectRatio = width / height;
                height = maxScreenResY;
                width = height * aspectRatio;
            }
            else if (width < height && width > maxScreenResX)
            {
                float aspectRatio = height / width;
                width = maxScreenResX;
                height = width * aspectRatio;
            }
            else if (width == height)
            {
                float maxSideLength = (maxScreenResX > maxScreenResY) ? maxScreenResX : maxScreenResY;
                if (width > maxSideLength)
                {
                    width = maxSideLength;
                    height = maxSideLength;
                }
            }

            Screen.SetResolution((int)width, (int)height, true);
            Kernel.Log($"[EnvironmentManager] Screen resolution is set: W: {Screen.width}, H: {Screen.height}", (int)LogChannel.Environment);
        }
    }
}