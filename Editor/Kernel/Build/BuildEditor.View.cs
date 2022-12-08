using System;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Morpheus.Editor.Build
{
    internal partial class BuildEditor : EditorWindow
    {
        private const string EditorName = "Build Editor";
        private const string VisualTreeAssetPath = "Assets/Framework/Editor/Kernel/Build/BuildEditor.uxml";

        private const string EditionListContainerName = "EditionListContainer";
        private const string EditionSaveBtnName = "EditionSaveBtn";
        private const string EditionAddBtnName = "EditionAddBtn";
        private const string EditionRemoveBtnName = "EditionRemoveBtn";
        private const string EditionConfigGroupName = "EditionConfigGroup";
        private const string EditionIdFieldName = "EditionIdField";
        private const string CustomVersionFieldName = "CustomVersionField";
        private const string BuildVersionFieldName = "BuildVersionField";
        private const string PlatformEnumFieldName = "PlatformEnumField";
        private const string AssetConfigEditBtnName = "AssetConfigEditBtn";
        private const string EditionBuildBtnName = "EditionBuildBtn";

        [MenuItem(EditorKernel.FrameworkMenuDirectory + EditorName, false, 18870000)]
        private static void ShowWindow()
        {
            GetWindow<BuildEditor>();
        }

        private IMGUIContainer editionListContainer;
        private EditionTreeView editionTreeView;

        private VisualElement editionConfigView;
        private IntegerField editionIdField;
        private TextInputBaseField<string> customVersionField;
        private IntegerField buildVersionField;
        private EnumField platformField;

        private BuildConfig config;
        private EditionItem selectedEditionitem;

        private void Awake()
        {
            titleContent.text = EditorName;
        }

        private void OnEnable()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VisualTreeAssetPath);
            if (visualTree == null)
            {
                return;
            }

            rootVisualElement.Add(visualTree.CloneTree());
            config = LoadConfig();
            SetEditionlListView();
            SetEditionOperationButtons();
            SetEditionConfigView();
            Refresh();
        }

        public BuildConfig FetchData()
        {
            config ??= new BuildConfig();
            config.editionConfigs = editionTreeView.FetchData();
            return config;
        }

        public void Refresh()
        {
            if (selectedEditionitem == null)
            {
                editionIdField.value = -1;
            }
            else
            {
                editionIdField.value = selectedEditionitem.Data.id;
                customVersionField.value = selectedEditionitem.Data.customVersion;
                buildVersionField.value = selectedEditionitem.Data.buildVersion;
                platformField.SetValueWithoutNotify(selectedEditionitem.Data.platform);
            }
            editionTreeView.Reload();
            editionConfigView.visible = selectedEditionitem != null;
        }

        private void SetEditionlListView()
        {
            editionTreeView = new EditionTreeView(new TreeViewState(), this);
            editionTreeView.SetData(config);
            editionListContainer = rootVisualElement.Q<IMGUIContainer>(EditionListContainerName);
            editionListContainer.onGUIHandler = OnEditionListContainerGui;
        }

        private void SetEditionOperationButtons()
        {
            Button btn = rootVisualElement.Q<Button>(EditionSaveBtnName);
            btn.clicked += OnEditionSaveClick;
            btn = rootVisualElement.Q<Button>(EditionAddBtnName);
            btn.clicked += OnEditionAddClick;
            btn = rootVisualElement.Q<Button>(EditionRemoveBtnName);
            btn.clicked += OnEditionRemoveClick;
            btn = rootVisualElement.Q<Button>(EditionBuildBtnName);
            btn.clicked += OnBuildClick;
        }

        private void SetEditionConfigView()
        {
            editionConfigView = rootVisualElement.Q<VisualElement>(EditionConfigGroupName);
            editionIdField = rootVisualElement.Q<IntegerField>(EditionIdFieldName);
            customVersionField = rootVisualElement.Q<TextInputBaseField<string>>(CustomVersionFieldName);
            customVersionField.RegisterValueChangedCallback(OnCustomVersionChanged);
            buildVersionField = rootVisualElement.Q<IntegerField>(BuildVersionFieldName);
            buildVersionField.RegisterValueChangedCallback(OnBuildVersionChanged);
            platformField = rootVisualElement.Q<EnumField>(PlatformEnumFieldName);
            platformField.RegisterValueChangedCallback(OnEditionPlatformChanged);

            Button btn = rootVisualElement.Q<Button>(AssetConfigEditBtnName);
            btn.clicked += OnAssetConfigEditClick;
        }

        private void OnEditionListContainerGui()
        {
            editionTreeView.OnGUI(editionListContainer.contentRect);
        }

        private void OnEditionSaveClick()
        {
            SaveConfig(FetchData());
            EditorUtility.DisplayDialog("[BuildEditor]", "Saved BuildConfig successfully.", "Confirm");
        }

        private void OnEditionAddClick()
        {
            EditionConfig editionConfig = new EditionConfig()
            {
                id = GetUniqueEditionId(editionTreeView.FetchData())
            };

            editionTreeView.AddEditionItem(editionConfig);
            Refresh();
        }

        private void OnEditionRemoveClick()
        {
            if (selectedEditionitem == null)
            {
                return;
            }

            selectedEditionitem.parent.children.Remove(selectedEditionitem);
            selectedEditionitem = null;
            Refresh();
        }

        public void OnEditionItemSelected(EditionItem item)
        {
            selectedEditionitem = item;
            Refresh();
        }

        public void OnEditionItemRename(EditionItem item, string newName)
        {
            string oldAssetEditorConfigPath = AssetEditor.GetAssetEditorConfigPath(item.Data);
            item.Rename(newName);

            if (File.Exists(oldAssetEditorConfigPath))
            {
                File.Move(oldAssetEditorConfigPath, AssetEditor.GetAssetEditorConfigPath(item.Data));
            }
            SaveConfig(FetchData());
        }

        private void OnCustomVersionChanged(ChangeEvent<string> evt)
        {
            if (selectedEditionitem == null)
            {
                return;
            }

            selectedEditionitem.Data.customVersion = evt.newValue;
        }

        private void OnBuildVersionChanged(ChangeEvent<int> evt)
        {
            if (selectedEditionitem == null)
            {
                return;
            }

            selectedEditionitem.Data.buildVersion = evt.newValue;
        }

        private void OnEditionPlatformChanged(ChangeEvent<Enum> evt)
        {
            if (selectedEditionitem == null)
            {
                return;
            }

            selectedEditionitem.Data.platform = (BuildTarget)evt.newValue;
        }

        private void OnAssetConfigEditClick()
        {
            AssetEditor.ShowWindow(selectedEditionitem.Data);
        }

        private void OnBuildClick()
        {
            if (selectedEditionitem == null)
            {
                DebugLogger.LogError($"[BuildEditor] There is no edition seleted.");
                return;
            }

            if (!BuildExecutable(selectedEditionitem.Data))
            {
                EditorUtility.DisplayDialog("[BuildEditor]", "Build the executable failed.", "Confirm");
                return;
            }

            ++selectedEditionitem.Data.buildVersion;
            SaveConfig(config);
            Refresh();
            EditorUtility.DisplayDialog("[BuildEditor]", "Build the executable successfully.", "Confirm");
        }
    }
}