using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blanketmen.Hypnos
{
    [Flags]
    public enum RunningVersion
    {
        Release = 0,
        Stage = 1,
        Development = 2,
        Test = 3
    }

    public class EnvironmentSettings : ScriptableObject
    {
        public const string UnityEngineAssemblyName = "Assembly-CSharp";
        public const string FrameworkAssemblyName = "Blanketmen.Hypnos";

#if TEST
        public const RunningVersion Version = RunningVersion.Test;
#elif DEVELOPMENT
        public const RunningVersion Version = RunningVersion.Development;
#elif STAGE
        public const RunningVersion Version = RunningVersion.Stage;
#else
        public const RunningVersion Version = RunningVersion.Release;
#endif

        public static string UniqueIdentifier => SystemInfo.deviceUniqueIdentifier;
        public static string OperatingSystem => SystemInfo.operatingSystem;

        public static int ProcessorCount => SystemInfo.processorCount;
        public static int MemorySize => SystemInfo.systemMemorySize;
        public static GraphicsDeviceType GraphicsApiType => SystemInfo.graphicsDeviceType;
        public static int GraphicsMemorySize => SystemInfo.graphicsMemorySize;

        public static string DataPath => Application.dataPath;
        public static string PersistentDataPath => Application.persistentDataPath;
        public static string StreamingAssetsPath => Application.streamingAssetsPath;

        public static string GameVersion => Application.version;
        public static int TargetFrameRate => Application.targetFrameRate;

        public static void SetSleepTimeout(int timeout) => Screen.sleepTimeout = timeout;
        public static void SetTargetFrameRate(int frameRate) => Application.targetFrameRate = frameRate;

        // NOTE: Limit the maximum resolution of the short side.
        [Conditional("UNITY_ANDROID"), Conditional("UNITY_IOS")]
        internal static void SetScreenResolution(float maxScreenResX, float maxScreenResY)
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
            Logging.Log($"[EnvironmentSettings] Screen resolution is set: W: {Screen.width}, H: {Screen.height}", (int)LogChannel.Environment);
        }

        public bool runInBackground = true;
        public bool neverSleepScreen = true;
        [Range(30f, 144f)] public int targetFrameRate = 60;
        public Vector2 maxScreenResolution = new Vector2(1334f, 750f);

        public void Initialize()
        {
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleepScreen ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting; // TODO: Will it be reset when switching scenes?
            Application.targetFrameRate = targetFrameRate;
            SetScreenResolution(maxScreenResolution.x, maxScreenResolution.y);
        }
    }
}