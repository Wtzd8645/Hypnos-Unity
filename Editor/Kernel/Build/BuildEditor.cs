using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Blanketmen.Hypnos.Editor.Build
{
    internal partial class BuildEditor
    {
        public const string WindowsExecutableExt = ".exe";
        public const string AndroidExecutableExt = ".apk";

        public const string BuildConfigDirectoryName = "Build";
        public const string BuildConfigFileName = "BuildConfig.xml";
        public const string ReleaseDirectoryName = "Release";
        public const string EditionDirectoryName = "{0}_{1}"; // 0: ID, 1: Name.
        public const string ExecutableDirectoryName = "Executable";

        public static string GetBuildConfigPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), BuildConfigDirectoryName, BuildConfigFileName);
        }

        public static string GetExecutablePath(EditionConfig config)
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                ReleaseDirectoryName,
                string.Format(EditionDirectoryName, config.id.ToString(), config.name),
                ExecutableDirectoryName,
                $"{config.customVersion}.{config.buildVersion}");
        }

        public static void SaveConfig(BuildConfig config)
        {
            try
            {
                DataArchiver archiver = new DataArchiver(CoreUtil.CreateSerializer(CoreSerializer.DotNetXml));
                archiver.Save(config, GetBuildConfigPath());
            }
            catch (Exception e)
            {
                Kernel.LogError($"[BuildEditor] Save BuildConfig failed. Exception: {e.Message}");
            }
        }

        public static BuildConfig LoadConfig()
        {
            try
            {
                DataArchiver archiver = new DataArchiver(CoreUtil.CreateSerializer(CoreSerializer.DotNetXml));
                return archiver.Load<BuildConfig>(GetBuildConfigPath());
            }
            catch (Exception e)
            {
                Kernel.LogWarning($"[BuildEditor] Load BuildConfig failed. Exception: {e.Message}");
                return new BuildConfig();
            }
        }

        private static int GetUniqueEditionId(List<EditionConfig> edtionConfigs)
        {
            if (edtionConfigs == null || edtionConfigs.Count == 0)
            {
                return 0;
            }

            int id = 0;
            while (true)
            {
                bool isDuplicate = false;
                foreach (EditionConfig config in edtionConfigs)
                {
                    if (config.id == id)
                    {
                        ++id;
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    break;
                }
            }
            return id;
        }

        public static string[] FetchEnabledScenePaths()
        {
            return EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        }

        public static bool BuildExecutable(EditionConfig config)
        {
            string outputPath = Path.Combine(GetExecutablePath(config), Application.productName);
            switch (config.platform)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                {
                    outputPath += WindowsExecutableExt;
                    break;
                }
                case BuildTarget.Android:
                {
                    outputPath += AndroidExecutableExt;
                    break;
                }
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = config.platform,
                targetGroup = BuildPipeline.GetBuildTargetGroup(config.platform),
                options = BuildOptions.Development,
                scenes = FetchEnabledScenePaths(),
                locationPathName = outputPath
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            return report.summary.result == BuildResult.Succeeded;
        }
    }
}