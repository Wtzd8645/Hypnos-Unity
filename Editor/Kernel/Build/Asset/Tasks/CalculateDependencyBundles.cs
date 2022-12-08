using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace Morpheus.Editor.Build
{
    internal class CalculateDependencyBundles : IBuildTask
    {
        private const string UnityDefaultResourcePath = "Library/unity default resources";
        private static readonly GUID BuiltInGuid = new GUID("0000000000000000f000000000000000");
        private static readonly string BuildInFileName = ((uint)"BuildIn".GetHashCode()).ToString() + AssetEditor.AssetBundleExt;

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        private IDependencyData m_DependencyData;

        [InjectContext(ContextUsage.In)]
        private IBuildParameters m_Parameters;

        [InjectContext(ContextUsage.In)]
        private IBundleBuildContent m_Content;

        [InjectContext(ContextUsage.InOut, true)]
        private IBundleExplictObjectLayout m_Layout;
#pragma warning restore 649

        private List<string> buffer = new List<string>(4096);

        public int Version { get { return 1; } }

        private uint GetHashCode(HashSet<string> bundleSet)
        {
            buffer.Clear();
            foreach (string bundle in bundleSet)
            {
                buffer.Add(bundle);
            }

            buffer.Sort();
            uint hash = 0;
            foreach (string bundle in buffer)
            {
                unchecked
                {
                    hash = hash * 31 + (uint)bundle.GetHashCode();
                }
            }
            return hash;
        }

        public ReturnCode Run()
        {
            Dictionary<GUID, AssetLoadInfo> assetInfoMap = m_DependencyData.AssetInfo;
            Dictionary<GUID, string> bundleMap = new Dictionary<GUID, string>(assetInfoMap.Count);
            foreach (KeyValuePair<string, List<GUID>> bundle in m_Content.BundleLayout)
            {
                foreach (GUID guid in bundle.Value)
                {
                    bundleMap[guid] = bundle.Key;
                }
            }

            HashSet<ObjectIdentifier> buildInDepObjIdSet = new HashSet<ObjectIdentifier>();
            Dictionary<ObjectIdentifier, HashSet<string>> extraDepInfoMap = new Dictionary<ObjectIdentifier, HashSet<string>>(4049);
            foreach (AssetLoadInfo assetInfo in assetInfoMap.Values)
            {
                string parentBundle = bundleMap[assetInfo.asset];
                foreach (ObjectIdentifier refObjId in assetInfo.referencedObjects)
                {
                    if (assetInfoMap.ContainsKey(refObjId.guid) || refObjId.filePath == UnityDefaultResourcePath)
                    {
                        continue;
                    }

                    if (refObjId.guid == BuiltInGuid)
                    {
                        buildInDepObjIdSet.Add(refObjId);
                        continue;
                    }

                    extraDepInfoMap.TryGetValue(refObjId, out HashSet<string> depInfo);
                    if (depInfo == null)
                    {
                        depInfo = new HashSet<string>();
                        extraDepInfoMap[refObjId] = depInfo;
                    }
                    depInfo.Add(parentBundle);
                }
            }

            // Scene objects need to be handled independently.
            foreach (KeyValuePair<GUID, SceneDependencyInfo> kvPair in m_DependencyData.SceneInfo)
            {
                string parentBundle = bundleMap[kvPair.Key];
                foreach (ObjectIdentifier refObjId in kvPair.Value.referencedObjects)
                {
                    if (assetInfoMap.ContainsKey(refObjId.guid) || refObjId.filePath == UnityDefaultResourcePath)
                    {
                        continue;
                    }

                    if (refObjId.guid == BuiltInGuid)
                    {
                        buildInDepObjIdSet.Add(refObjId);
                        continue;
                    }

                    extraDepInfoMap.TryGetValue(refObjId, out HashSet<string> depInfo);
                    if (depInfo == null)
                    {
                        depInfo = new HashSet<string>();
                        extraDepInfoMap[refObjId] = depInfo;
                    }
                    depInfo.Add(parentBundle);
                }
            }

            m_Layout ??= new BundleExplictObjectLayout();

            foreach (ObjectIdentifier objId in buildInDepObjIdSet)
            {
                m_Layout.ExplicitObjectLocation.Add(objId, BuildInFileName);
            }

            foreach (KeyValuePair<ObjectIdentifier, HashSet<string>> kvPair in extraDepInfoMap)
            {
                if (kvPair.Value.Count > 1)
                {
                    m_Layout.ExplicitObjectLocation.Add(kvPair.Key, $"{GetHashCode(kvPair.Value)}{AssetEditor.AssetBundleExt}");
                }
            }

            if (m_Layout.ExplicitObjectLocation.Count == 0)
            {
                m_Layout = null;
            }
            return ReturnCode.Success;
        }
    }
}
