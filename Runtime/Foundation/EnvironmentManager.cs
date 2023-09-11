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

    public class EnvironmentManager
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

        public static Vector2 ScreenResolution => new Vector2(Screen.width, Screen.height);
        public static void SetSleepTimeout(int timeout) => Screen.sleepTimeout = timeout;
        public static void SetTargetFrameRate(int frameRate) => Application.targetFrameRate = frameRate;

        // NOTE: Limit the maximum resolution of the short side.
        /// <summary>
        /// Set the screen resolution based on the provided maximum limits.
        /// <para>This will make the resolution not exceed these dimensions while maintaining the current aspect ratio.</para>
        /// <para>Intended for mobile platforms (Android and iOS).</para>
        /// </summary>
        /// <param name="maxRes">Maximum resolution limits for width and height.</param>
        [Conditional("UNITY_ANDROID"), Conditional("UNITY_IOS")]
        public static void SetScreenResolution(Vector2 maxRes)
        {
            float width = Screen.width;
            float height = Screen.height;
            if (width > height && height > maxRes.y)
            {
                float aspectRatio = width / height;
                height = maxRes.y;
                width = height * aspectRatio;
            }
            else if (width < height && width > maxRes.x)
            {
                float aspectRatio = height / width;
                width = maxRes.x;
                height = width * aspectRatio;
            }
            else if (width == height)
            {
                float maxSideLength = (maxRes.x > maxRes.y) ? maxRes.x : maxRes.y;
                if (width > maxSideLength)
                {
                    width = maxSideLength;
                    height = maxSideLength;
                }
            }

            Screen.SetResolution((int)width, (int)height, true);
            Logging.Log($"[EnvironmentManager] Screen resolution is set. Resoltion: {Screen.width}, {Screen.height}", (int)LogChannel.Environment);
        }

        public static void Set(EnvironmentConfig config)
        {
            Application.runInBackground = config.runInBackground;
            Screen.sleepTimeout = config.neverSleepScreen ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting; // TODO: Will it be reset when switching scenes?
            Application.targetFrameRate = config.targetFrameRate;
            SetScreenResolution(config.maxScreenResolution);
        }
    }
}