using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace Blanketmen.Hypnos.Editor.Build
{
    internal partial class AssetEditor
    {
        public const string AssetBundleExt = ".res";
        public const string AssetEditorConfigFileName = "AssetEditorConfig_{0}_{1}.xml";
        public const string AssetBundleOutputDirectoryName = "AssetBundles";

        public static string GetAssetEditorConfigPath(EditionConfig editionConfig)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), BuildEditor.BuildConfigDirectoryName, string.Format(AssetEditorConfigFileName, editionConfig.id.ToString(), editionConfig.name));
        }

        public static string GetAssetBundlesOutputPath(AssetEditorConfig editorConfig, EditionConfig editionConfig)
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                BuildEditor.ReleaseDirectoryName,
                string.Format(BuildEditor.EditionDirectoryName, editionConfig.id.ToString(), editionConfig.name),
                AssetBundleOutputDirectoryName,
                editorConfig.version.ToString());
        }

        public static string GetAssetConfigOutputPath(AssetEditorConfig editorConfig, EditionConfig editionConfig)
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                BuildEditor.ReleaseDirectoryName,
                string.Format(BuildEditor.EditionDirectoryName, editionConfig.id.ToString(), editionConfig.name),
                AssetBundleOutputDirectoryName,
                editorConfig.version.ToString(),
                ResourceManager.AssetConfigFileName);
        }

        public static void SaveConfig(AssetEditorConfig editorConfig, EditionConfig editionConfig)
        {
            try
            {
                ResourceManager.AssetConfigArchiver.Save(editorConfig, GetAssetEditorConfigPath(editionConfig));
            }
            catch (Exception e)
            {
                Logging.Error($"Save AssetConfig failed. Exception: {e.Message}", nameof(AssetEditor));
            }
        }

        public static AssetEditorConfig LoadConfig(EditionConfig editionConfig)
        {
            try
            {
                return ResourceManager.AssetConfigArchiver.Load<AssetEditorConfig>(GetAssetEditorConfigPath(editionConfig));
            }
            catch (Exception e)
            {
                Logging.Warning($"[AssetEditor] Load AssetConfig failed. Exception: {e.Message}");
                return null;
            }
        }

        private static void FetchAssetInfos(AssetEditorData data, List<string> idResult, List<string> pathResult)
        {
            if (data.assetType != AssetType.Directory)
            {
                idResult.Add(data.assetId);
                pathResult.Add(data.assetPath);
            }

            if (data.subAssets == null)
            {
                return;
            }

            foreach (AssetEditorData subData in data.subAssets)
            {
                FetchAssetInfos(subData, idResult, pathResult);
            }
        }

        private static BundleBuildContent GetAssetBundleBuildContent(AssetEditorConfig editorConfig)
        {
            List<AssetGroupEditorData> groupDatas = editorConfig.groupDatas;
            List<string> assetIdBuf = new List<string>(1024);
            List<string> assetPathBuf = new List<string>(1024);
            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[groupDatas.Count];
            for (int i = 0, count = groupDatas.Count; i < count; ++i)
            {
                if (groupDatas[i].assetDatas == null)
                {
                    throw new Exception($"[AssetEditor] Group content can't be empty. GroupId: {groupDatas[i].groupId}");
                }

                assetBundleBuilds[i].assetBundleName = groupDatas[i].groupId + AssetBundleExt;
                foreach (AssetEditorData data in groupDatas[i].assetDatas)
                {
                    FetchAssetInfos(data, assetIdBuf, assetPathBuf);
                }
                assetBundleBuilds[i].addressableNames = assetIdBuf.ToArray();
                assetBundleBuilds[i].assetNames = assetPathBuf.ToArray();
                assetIdBuf.Clear();
                assetPathBuf.Clear();
            }
            return new BundleBuildContent(assetBundleBuilds);
        }

        private static IList<IBuildTask> GetAssetBundleBuildTasks()
        {
            List<IBuildTask> buildTasks = new List<IBuildTask>(32)
            {
                // Setup
                new SwitchToBuildPlatform(),
                new RebuildSpriteAtlasCache(),

                // Player Scripts
                new BuildPlayerScripts(),
                new PostScriptsCallback(),

                // Dependency
                new CalculateSceneDependencyData(),
#if UNITY_2019_3_OR_NEWER
                new CalculateCustomDependencyData(),
#endif
                new CalculateAssetDependencyData(),
                new StripUnusedSpriteSources(),
                //buildTasks.Add(new CreateBuiltInShadersBundle("BuildInShaders.res"));
                //buildTasks.Add(new CreateMonoScriptBundle("MonoScripts.res"));
                new CalculateDependencyBundles(),
                new PostDependencyCallback(),

                // Packing
                new GenerateBundlePacking(),
                new UpdateBundleObjectLayout(),
                new GenerateBundleCommands(),
                new GenerateSubAssetPathMaps(),
                new GenerateBundleMaps(),
                new PostPackingCallback(),

                // Writing
                new WriteSerializedFiles(),
                new ArchiveAndCompressBundles(),
                new AppendBundleHash(),
                new GenerateLinkXml(),
                new PostWritingCallback()
            };
            return buildTasks;
        }

        private static void GetAssetDatas(string groupId, List<AssetEditorData> assetEditorDatas, List<AssetData> result)
        {
            if (assetEditorDatas == null)
            {
                return;
            }

            foreach (AssetEditorData assetEditorData in assetEditorDatas)
            {
                if (assetEditorData.assetType == AssetType.Directory)
                {
                    GetAssetDatas(groupId, assetEditorData.subAssets, result);
                    continue;
                }

                AssetData assetData = new AssetData
                {
                    assetId = string.IsNullOrEmpty(assetEditorData.assetId) ? assetEditorData.assetPath : assetEditorData.assetId,
                    assetPath = assetEditorData.assetPath,
                    groupId = groupId + AssetBundleExt
                };
                result.Add(assetData);
            }
        }

        private static bool BuildFastEditorAssetConfig(AssetEditorConfig editorConfig)
        {
            List<AssetData> assetDatas = new List<AssetData>(32768);
            foreach (AssetGroupEditorData groupEditorData in editorConfig.groupDatas)
            {
                GetAssetDatas(groupEditorData.groupId, groupEditorData.assetDatas, assetDatas);
            }
            AssetManifest assetManifest = new AssetManifest
            {
                assetDatas = assetDatas.ToArray()
            };
            AssetRegistry config = new AssetRegistry
            {
                version = editorConfig.version,
                assetManifest = assetManifest
            };

            try
            {
                string outputPath = Path.Combine(Application.persistentDataPath, ResourceManager.ResourcesDirectoryName, ResourceManager.AssetConfigFileName);
                ResourceManager.AssetConfigArchiver.Save(config, outputPath);
                Logging.Info($"Build FastEditor AssetConfig successfully. OutputPath: {outputPath}", (int)LogChannel.Resource);
                return true;
            }
            catch (Exception e)
            {
                Logging.Error($"Build FastEditor AssetConfig failed. Exception: {e.Message}", nameof(AssetEditor));
                return false;
            }
        }

        private static void CopyAssetBundles(AssetEditorConfig editorConfig, EditionConfig editionConfig, string targetDirectory)
        {
            if (editorConfig == null || editionConfig == null)
            {
                Logging.Error("AssetEditorConfig or EditionConfig is null.", nameof(AssetEditor));
                return;
            }

            string assetBundlesDir = GetAssetBundlesOutputPath(editorConfig, editionConfig);
            if (!Directory.Exists(assetBundlesDir))
            {
                Logging.Error($"Assetbundles directory does not exist. Version: {editorConfig.version}, Path: {assetBundlesDir}", nameof(AssetEditor));
                return;
            }

            CleanAssetBundlesCopy(targetDirectory);
            Directory.CreateDirectory(targetDirectory);

            string[] files = Directory.GetFiles(assetBundlesDir);
            foreach (string file in files)
            {
                File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)));
            }
            Logging.Info($"Copy AssetBundles successfully. Version: {editorConfig.version}, Path: {targetDirectory}", (int)LogChannel.Resource);
        }

        private static void CleanAssetBundlesCopy(string targetDirectory)
        {
            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(targetDirectory, true);
            }
            Logging.Info($"Clean AssetBundles copy successfully. Path: {targetDirectory}", (int)LogChannel.Resource);
        }

        private static void BuildAssetConfig(AssetEditorConfig editorConfig, EditionConfig editionConfig, IBundleBuildResults buildResults)
        {
            int bundleCount = 0;
            AssetGroupData[] groupDatas = new AssetGroupData[buildResults.BundleInfos.Count];
            foreach (KeyValuePair<string, BundleDetails> infos in buildResults.BundleInfos)
            {
                BundleDetails details = infos.Value;
                AssetGroupData data = new AssetGroupData
                {
                    groupId = infos.Key,
                    crc = details.Crc,
                    hash = details.Hash,
                    fileName = infos.Key,
                    fileOffset = 0ul,
                    dependencies = details.Dependencies
                };
                groupDatas[bundleCount++] = data;
            }
            AssetGroupManifest groupManifest = new AssetGroupManifest
            {
                assetGroupDatas = groupDatas
            };

            List<AssetData> assetDatas = new List<AssetData>(32768);
            foreach (AssetGroupEditorData groupEditorData in editorConfig.groupDatas)
            {
                GetAssetDatas(groupEditorData.groupId, groupEditorData.assetDatas, assetDatas);
            }
            AssetManifest assetManifest = new AssetManifest
            {
                assetDatas = assetDatas.ToArray()
            };
            AssetRegistry config = new AssetRegistry
            {
                version = editorConfig.version,
                assetGroupManifest = groupManifest,
                assetManifest = assetManifest
            };

            try
            {
                string outputPath = GetAssetConfigOutputPath(editorConfig, editionConfig);
                ResourceManager.AssetConfigArchiver.Save(config, outputPath);
            }
            catch (Exception e)
            {
                Logging.Error($"Build AssetConfig failed. Exception: {e.Message}", nameof(AssetEditor));
            }
        }

        public static bool BuildAssetBundles(AssetEditorConfig editorConfig, EditionConfig editionConfig)
        {
            string outputPath = GetAssetBundlesOutputPath(editorConfig, editionConfig);
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
            Directory.CreateDirectory(outputPath);

            BundleBuildParameters buildParams = new BundleBuildParameters(
                    editionConfig.platform,
                    BuildPipeline.GetBuildTargetGroup(editionConfig.platform),
                    outputPath);
            ReturnCode errorCode = ContentPipeline.BuildAssetBundles(
                buildParams,
                GetAssetBundleBuildContent(editorConfig),
                out IBundleBuildResults results,
                GetAssetBundleBuildTasks());
            if (errorCode != ReturnCode.Success)
            {
                Logging.Error($"Build AssetBundles failed. ErrorCode: {errorCode}", nameof(AssetEditor));
                return false;
            }

            BuildAssetConfig(editorConfig, editionConfig, results);
            Logging.Info($"Build AssetBundles successfully. OutputPath: {outputPath}", (int)LogChannel.Resource);
            return true;
        }
    }
}