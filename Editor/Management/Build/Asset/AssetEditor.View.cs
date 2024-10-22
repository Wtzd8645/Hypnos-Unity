using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blanketmen.Hypnos.Editor.Build
{
    internal partial class AssetEditor : EditorWindow
    {
        private const string EditorViewName = "Asset Editor";
        private const string VisualTreeAssetName = "AssetEditor";

        private const string VersionFieldName = "VersionField";
        private const string EditionInfoFieldName = "EditionInfoField";
        private const string EditorMenuName = "EditorMenu";
        private const string BuildMenuName = "BuildMenu";
        private const string AssetGroupContainerName = "AssetGroupContainer";
        private const string AddGroupBtnName = "AddGroupBtn";
        private const string SaveBtnName = "SaveBtn";

        public static void ShowWindow(EditionConfig editionConfig)
        {
            GetWindow<AssetEditor>().SetEditionConfig(editionConfig);
        }

        private MultiColumnHeader multiColumnHeader;
        private TextInputBaseField<string> versionField;
        private Label editionInfoLabel;
        private AssetTreeView assetGroupTreeView;

        private EditionConfig editionConfig;

        private void OnEnable()
        {
            titleContent.text = EditorViewName;
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>(VisualTreeAssetName);
            if (visualTree == null)
            {
                return;
            }

            visualTree.CloneTree(rootVisualElement);
            SetAssetOperations();
            SetAssetGroupColumns();
            SetAssetGroupView();
            Refresh();
        }

        public AssetEditorConfig FetchData()
        {
            if (editionConfig == null)
            {
                Logging.Error("EditionConfig is null.", nameof(AssetEditor));
                return null;
            }

            AssetEditorConfig editorConfig = new AssetEditorConfig();
            if (!int.TryParse(versionField.value, out editorConfig.version))
            {
                Logging.Error("AssetEditorConfig version can only be an integer.", nameof(AssetEditor));
            }
            editorConfig.groupDatas = assetGroupTreeView.FetchData();
            return editorConfig;
        }

        public void Refresh()
        {
            if (editionConfig != null)
            {
                editionInfoLabel.text = $"ID: {editionConfig.id}  Name: {editionConfig.name}  Platform: {editionConfig.platform}";

                AssetEditorConfig editorConfig = LoadConfig(editionConfig);
                if (editorConfig == null)
                {
                    versionField.value = "0";
                    assetGroupTreeView.SetData(null);
                }
                else
                {
                    versionField.value = editorConfig.version.ToString();
                    assetGroupTreeView.SetData(editorConfig.groupDatas);
                }
            }
            assetGroupTreeView.Reload();
        }

        public void SetEditionConfig(EditionConfig config)
        {
            editionConfig = config;
            Refresh();
        }

        private void SetAssetOperations()
        {
            ToolbarMenu toolbarMenu = rootVisualElement.Q<ToolbarMenu>(EditorMenuName);
            toolbarMenu.menu.AppendAction("Generate FastEditor AssetConfig", OnGenerateFastEditorAssetConfigClick);
            toolbarMenu.menu.AppendAction("Copy AssetBundles to StreamingPath", OnCopyAssetBundlesToStreamingPathClick);
            toolbarMenu.menu.AppendAction("Clean StreamingPath AssetBundles Copy",
                OnCleanStreamingPathAssetBundlesCopyClick);
            toolbarMenu.menu.AppendAction("Copy AssetBundles to PersistentPath", OnCopyAssetBundlesToPersistentPathClick);
            toolbarMenu.menu.AppendAction("Clean PersistentPath AssetBundles Copy",
                OnCleanPersistentPathAssetBundlesCopyClick);

            toolbarMenu = rootVisualElement.Q<ToolbarMenu>(BuildMenuName);
            toolbarMenu.menu.AppendAction("Build AssetBundles", OnBuildAssetBundlesClick);
            toolbarMenu.menu.AppendAction("Rebuild AssetBundles", OnRebuildAssetBundlesClick);

            Button btn = rootVisualElement.Q<Button>(AddGroupBtnName);
            btn.clicked += OnAddGroupClick;
            btn = rootVisualElement.Q<Button>(SaveBtnName);
            btn.clicked += OnSaveClick;
        }

        private void SetAssetGroupColumns()
        {
            MultiColumnHeaderState.Column[] columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = EditorGUIUtility.TrTextContent("Group / Asset Name"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    autoResize = true,
                    allowToggleVisibility = false,
                    width = 200f
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = EditorGUIUtility.TrTextContent("Path"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    autoResize = true,
                    allowToggleVisibility = false,
                    width = 300f
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = EditorGUIUtility.TrTextContent("Action"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    autoResize = true,
                    allowToggleVisibility = false,
                    width = 85f,
                    minWidth = 85f
                }
            };

            multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(columns));
            multiColumnHeader.ResizeToFit();
        }

        private void SetAssetGroupView()
        {
            assetGroupTreeView = new AssetTreeView(new TreeViewState(), multiColumnHeader);
            IMGUIContainer imGui = rootVisualElement.Q<IMGUIContainer>(AssetGroupContainerName);
            imGui.onGUIHandler = () => { assetGroupTreeView.OnGUI(imGui.contentRect); };

            versionField = rootVisualElement.Q<TextInputBaseField<string>>(VersionFieldName);
            editionInfoLabel = rootVisualElement.Q<Label>(EditionInfoFieldName);
        }

        private void OnAddGroupClick()
        {
            assetGroupTreeView.AddGroupItem();
            assetGroupTreeView.Reload();
        }

        private void OnSaveClick()
        {
            if (editionConfig == null)
            {
                Logging.Error("The edition config is null, please reopen the window.", nameof(AssetEditor));
                return;
            }

            SaveConfig(FetchData(), editionConfig);
            EditorUtility.DisplayDialog("[AssetEditor]", "Save AssetConfig successfully.", "Confirm");
        }

        private void OnGenerateFastEditorAssetConfigClick(DropdownMenuAction action)
        {
            AssetEditorConfig editorConfig = FetchData();
            if (editorConfig == null || editionConfig == null)
            {
                return;
            }

            if (!BuildFastEditorAssetConfig(editorConfig))
            {
                EditorUtility.DisplayDialog("[AssetEditor]", $"Generate FastEditor AssetConfig failed.", "Confirm");
                return;
            }

            EditorUtility.DisplayDialog("[AssetEditor]", "Generate FastEditor AssetConfig successfully.", "Confirm");
        }

        private void OnCopyAssetBundlesToStreamingPathClick(DropdownMenuAction action)
        {
            AssetEditorConfig editorConfig = new AssetEditorConfig();
            if (!int.TryParse(versionField.value, out editorConfig.version))
            {
                Logging.Error("AssetEditorConfig version can only be an integer.", nameof(AssetEditor));
                return;
            }

            string targetDir = Path.Combine(Application.streamingAssetsPath, ResourceManager.ResourcesDirectoryName);
            CopyAssetBundles(editorConfig, editionConfig, targetDir);
            AssetDatabase.Refresh();
        }

        private void OnCleanStreamingPathAssetBundlesCopyClick(DropdownMenuAction action)
        {
            string targetDir = Path.Combine(Application.streamingAssetsPath, ResourceManager.ResourcesDirectoryName);
            CleanAssetBundlesCopy(targetDir);
            AssetDatabase.Refresh();
        }

        private void OnCopyAssetBundlesToPersistentPathClick(DropdownMenuAction action)
        {
            AssetEditorConfig editorConfig = new AssetEditorConfig();
            if (!int.TryParse(versionField.value, out editorConfig.version))
            {
                Logging.Error("AssetEditorConfig version can only be an integer.", nameof(AssetEditor));
                return;
            }

            string targetDir = Path.Combine(Application.persistentDataPath, ResourceManager.ResourcesDirectoryName);
            CopyAssetBundles(editorConfig, editionConfig, targetDir);
        }

        private void OnCleanPersistentPathAssetBundlesCopyClick(DropdownMenuAction action)
        {
            string targetDir = Path.Combine(Application.persistentDataPath, ResourceManager.ResourcesDirectoryName);
            CleanAssetBundlesCopy(targetDir);
        }

        private void OnBuildAssetBundlesClick(DropdownMenuAction action)
        {
            AssetEditorConfig editorConfig = FetchData();
            if (editorConfig == null || editionConfig == null)
            {
                return;
            }

            if (!BuildAssetBundles(editorConfig, editionConfig))
            {
                EditorUtility.DisplayDialog("[AssetEditor]", $"Build AssetBundles failed.", "Confirm");
                return;
            }

            SaveConfig(editorConfig, editionConfig);
            Refresh();
            EditorUtility.DisplayDialog("[AssetEditor]", "Build AssetBundles successfully.", "Confirm");
        }

        private void OnRebuildAssetBundlesClick(DropdownMenuAction action)
        {
            // TODO: Implement.
        }
    }
}