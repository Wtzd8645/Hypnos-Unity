using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Morpheus.Editor.Build
{
    internal class AssetTreeView : TreeView
    {
        private const string GenericDragId = "GenericDragColumnDragging";

        private readonly HashSet<string> groupIdSet = new HashSet<string>();
        private readonly HashSet<string> assetIdSet = new HashSet<string>();
        private readonly HashSet<string> assetPathSet = new HashSet<string>();

        private int itemId;
        private TreeViewItem abRootItem;

        public AssetTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader) : base(state, multicolumnHeader)
        {
            showBorder = true;
            abRootItem = new TreeViewItem()
            {
                id = FetchItemId(),
                displayName = "Root",
                depth = -1,
                parent = null
            };
        }

        public int FetchItemId()
        {
            return itemId++;
        }

        public List<AssetGroupEditorData> FetchData()
        {
            if (!abRootItem.hasChildren)
            {
                return null;
            }

            List<AssetGroupEditorData> groupDatas = new List<AssetGroupEditorData>(abRootItem.children.Count);
            foreach (TreeViewItem child in abRootItem.children)
            {
                groupDatas.Add((child as AssetGroupTreeItem).FetchData());
            }
            return groupDatas;
        }

        public void SetData(List<AssetGroupEditorData> groupDatas)
        {
            groupIdSet.Clear();
            assetIdSet.Clear();
            assetPathSet.Clear();
            if (abRootItem.hasChildren)
            {
                abRootItem.children.Clear();
            }

            if (groupDatas == null)
            {
                Reload();
                return;
            }

            int treeDepth = abRootItem.depth + 1;
            foreach (AssetGroupEditorData data in groupDatas)
            {
                AddGroupItemRecursive(data, treeDepth);
            }
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            return abRootItem;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0, col = args.GetNumVisibleColumns(); i < col; ++i)
            {
                (args.item as AssetItemBase).OnGui(args.GetCellRect(i), args.GetColumn(i));
            }
        }

        protected override void ContextClickedItem(int id)
        {
            IList<int> selectedItemIds = GetSelection();
            if (selectedItemIds.Count <= 0)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove"), false, OnItemRemoveClick, selectedItemIds);
            menu.ShowAsContext();
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return args.draggedItem.depth > -1;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            TreeViewItem[] draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToArray();
            DragAndDrop.SetGenericData(GenericDragId, draggedRows);
            DragAndDrop.StartDrag(draggedRows.Length == 1 ? draggedRows[0].displayName : "< Multiple >");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if ((args.parentItem as AssetGroupTreeItem) == null)
            {
                return DragAndDropVisualMode.None;
            }

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                {
                    if (!args.performDrop)
                    {
                        return DragAndDropVisualMode.Move;
                    }

                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        OnAssetDropped(DragAndDrop.objectReferences, args.parentItem);
                        return DragAndDropVisualMode.Move;
                    }

                    // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
                    OnItemDropped(DragAndDrop.GetGenericData(GenericDragId) as TreeViewItem[], args.parentItem);
                    return DragAndDropVisualMode.Move;
                }
                case DragAndDropPosition.OutsideItems:
                default:
                {
                    return DragAndDropVisualMode.None;
                }
            }
        }

        private void OnAssetDropped(Object[] droppedItems, TreeViewItem groupItem)
        {
            foreach (Object obj in droppedItems)
            {
                AddAssetItemRecursive(AssetDatabase.GetAssetPath(obj), groupItem, true);
            }

            if (droppedItems.Length > 0)
            {
                Reload();
            }
        }

        private void OnItemDropped(TreeViewItem[] droppedItems, TreeViewItem groupItem)
        {
            foreach (TreeViewItem item in droppedItems)
            {
                TreeViewItem oriGroupItem = item.parent;
                while (oriGroupItem != null && (oriGroupItem as AssetGroupTreeItem) == null)
                {
                    oriGroupItem = oriGroupItem.parent;
                }

                if (oriGroupItem == groupItem)
                {
                    continue;
                }

                ChangeAssetParentItem(item as AssetTreeItem, groupItem);
            }

            if (droppedItems.Length > 0)
            {
                Reload();
            }
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            TreeViewItem item = FindItem(args.itemID, abRootItem);
            if (item is AssetGroupTreeItem groupItem)
            {
                if (groupItem.Data.groupId == args.newName)
                {
                    return;
                }

                if (!groupIdSet.Add(args.newName))
                {
                    Logger.LogError($"[AssetTreeView] GroupId can't be duplicate. GroupId: {args.newName}");
                    return;
                }

                groupIdSet.Remove(groupItem.Data.groupId);
                groupItem.Data.groupId = args.newName;
                return;
            }

            if (item is AssetTreeItem assetItem)
            {
                if (assetItem.Data.assetId == args.newName)
                {
                    return;
                }

                if (!assetIdSet.Add(args.newName))
                {
                    Logger.LogError($"[AssetTreeView] AssetId can't be duplicate. AssetId: {args.newName}");
                    return;
                }

                assetIdSet.Remove(assetItem.Data.assetId);
                assetItem.Data.assetId = args.newName;
            }
        }

        private void UpdateTreeDepth(TreeViewItem item, int treeDepth)
        {
            item.depth = treeDepth++;
            if (item.hasChildren)
            {
                foreach (TreeViewItem child in item.children)
                {
                    UpdateTreeDepth(child, treeDepth);
                }
            }
        }

        private TreeViewItem FindNearestParentItem(string assetPath, TreeViewItem parent)
        {
            if (!parent.hasChildren)
            {
                return parent;
            }

            foreach (TreeViewItem child in parent.children)
            {
                AssetTreeItem childItem = child as AssetTreeItem;
                if (childItem.Data.assetType != AssetType.Directory)
                {
                    continue;
                }

                if (assetPath.StartsWith(childItem.Data.assetPath))
                {
                    parent = FindNearestParentItem(assetPath, child);
                    break;
                }
            }
            return parent;
        }

        // Note: If dirItem contains targetItem, it will cause an infinite loop.
        private void ReorganizeDirectory(AssetTreeItem targetItem, TreeViewItem dirItem)
        {
            if (dirItem == abRootItem)
            {
                return;
            }

            List<TreeViewItem> children = dirItem.children;
            if (children == null)
            {
                ReorganizeDirectory(targetItem, dirItem.parent);
                return;
            }

            for (int i = children.Count - 1; i >= 0; --i)
            {
                AssetTreeItem childItem = children[i] as AssetTreeItem;
                if (childItem.Data.assetPath.StartsWith(targetItem.Data.assetPath))
                {
                    TreeViewItem nerestParent = FindNearestParentItem(childItem.Data.assetPath, targetItem);
                    childItem.parent.children.Remove(childItem);
                    childItem.parent = nerestParent;
                    nerestParent.AddChild(childItem);
                }
            }
            ReorganizeDirectory(targetItem, dirItem.parent);
        }

        private void ChangeAssetParentItem(AssetTreeItem item, TreeViewItem parent)
        {
            parent = FindNearestParentItem(item.Data.assetPath, parent);
            item.parent.children.Remove(item);
            item.parent = parent;
            if (item.Data.assetType == AssetType.Directory)
            {
                ReorganizeDirectory(item, parent);
            }
            UpdateTreeDepth(item, parent.depth + 1);
            parent.AddChild(item);
        }

        public void AddGroupItem(string groupId = "")
        {
            const string DefaultId = "New Group";
            string finalId = string.IsNullOrEmpty(groupId) ? DefaultId : groupId;
            int count = 2;
            while (true)
            {
                if (groupIdSet.Add(finalId))
                {
                    break;
                }

                finalId = $"{DefaultId} {count++}";
            }

            AssetGroupTreeItem item = new AssetGroupTreeItem(finalId, this, abRootItem.depth + 1);
            abRootItem.AddChild(item);
            BeginRename(item);
        }

        private void AddGroupItemRecursive(AssetGroupEditorData data, int treeDepth)
        {
            if (!groupIdSet.Add(data.groupId))
            {
                Logger.LogError($"Duplicate group id are not allowed. GroupId: {data.groupId}");
                return;
            }

            AssetGroupTreeItem groupItem = new AssetGroupTreeItem(data, this, treeDepth++);
            abRootItem.AddChild(groupItem);

            if (data.assetDatas == null)
            {
                return;
            }

            foreach (AssetEditorData assetData in data.assetDatas)
            {
                AddAssetItemRecursive(assetData, groupItem, treeDepth);
            }
        }

        public void OnGroupItemRemoved(AssetGroupTreeItem item)
        {
            groupIdSet.Remove(item.Data.groupId);
            if (!item.hasChildren)
            {
                return;
            }

            foreach (TreeViewItem child in item.children)
            {
                OnAssetItemRemoved(child as AssetTreeItem);
            }
        }

        private void AddAssetItem(string assetPath, TreeViewItem parent)
        {
            if (!assetPathSet.Add(assetPath))
            {
                Logger.LogWarning($"Duplicate asset path are not allowed. AssetPath: {assetPath}");
                return;
            }

            string assetId = Path.GetFileName(assetPath);
            assetId = assetIdSet.Add(assetId) ? assetId : assetPath;
            parent.AddChild(new AssetTreeItem(assetId, assetPath, this, parent.depth + 1));
        }

        public void AddAssetItemRecursive(string assetPath, TreeViewItem parent, bool isReorganize)
        {
            assetPath = assetPath.Replace('\\', '/');
            parent = FindNearestParentItem(assetPath, parent);
            if (!Directory.Exists(assetPath))
            {
                AddAssetItem(assetPath, parent);
                return;
            }

            if (!assetPathSet.Add(assetPath))
            {
                Logger.LogWarning($"Duplicate assets are not allowed. AssetPath: {assetPath}");
                return;
            }

            AssetTreeItem dirItem = new AssetTreeItem(assetPath, assetPath, this, parent.depth + 1);
            string[] directories = Directory.GetDirectories(assetPath);
            foreach (string dir in directories)
            {
                AddAssetItemRecursive(dir, dirItem, false);
            }

            string[] files = Directory.GetFiles(assetPath);
            foreach (string file in files)
            {
                string fileExt = Path.GetExtension(file);
                if (fileExt == ".meta" || fileExt == ".cs")
                {
                    continue;
                }

                AddAssetItemRecursive(file, dirItem, false);
            }

            if (isReorganize)
            {
                ReorganizeDirectory(dirItem, parent);
                UpdateTreeDepth(dirItem, dirItem.depth);
            }
            parent.AddChild(dirItem);
        }

        public void AddAssetItemRecursive(AssetEditorData data, TreeViewItem parent, int treeDepth)
        {
            if (!assetPathSet.Add(data.assetPath))
            {
                Logger.LogError($"Duplicate assets are not allowed. AssetPath: {data.assetPath}");
                return;
            }

            if (!assetIdSet.Add(data.assetId))
            {
                data.assetId = data.assetPath;
            }

            AssetTreeItem item = new AssetTreeItem(data, this, treeDepth++);
            parent.AddChild(item);

            if (data.subAssets == null)
            {
                return;
            }

            foreach (AssetEditorData subData in data.subAssets)
            {
                AddAssetItemRecursive(subData, item, treeDepth);
            }
        }

        public void OnAssetItemRemoved(AssetTreeItem item)
        {
            assetIdSet.Remove(item.Data.assetId);
            assetPathSet.Remove(item.Data.assetPath);
            if (!item.hasChildren)
            {
                return;
            }

            foreach (TreeViewItem child in item.children)
            {
                OnAssetItemRemoved(child as AssetTreeItem);
            }
        }

        private void OnItemRemoveClick(object userData)
        {
            IList<int> selectedItemIds = userData as IList<int>;
            foreach (int itemId in selectedItemIds)
            {
                TreeViewItem item = FindItem(itemId, abRootItem);
                item.parent.children.Remove(item);
                if (item is AssetGroupTreeItem groupItem)
                {
                    OnGroupItemRemoved(groupItem);
                    continue;
                }

                if (item is AssetTreeItem assetItem)
                {
                    OnAssetItemRemoved(assetItem);
                }
            }
            Reload();
        }
    }
}