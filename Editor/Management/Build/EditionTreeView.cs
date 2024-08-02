using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Blanketmen.Hypnos.Editor.Build
{
    internal class EditionTreeView : TreeView
    {
        private BuildEditor buildEditor;
        private int itemId;
        private TreeViewItem abRootItem;

        public EditionTreeView(TreeViewState state, BuildEditor editor) : base(state)
        {
            showBorder = true;
            abRootItem = new TreeViewItem()
            {
                id = FetchItemId(),
                displayName = "Root",
                depth = -1,
                parent = null
            };
            buildEditor = editor;
        }

        public int FetchItemId()
        {
            return itemId++;
        }

        public List<EditionConfig> FetchData()
        {
            if (!abRootItem.hasChildren)
            {
                return null;
            }

            List<EditionConfig> editionConfigs = new List<EditionConfig>(abRootItem.children.Count);
            foreach (TreeViewItem child in abRootItem.children)
            {
                editionConfigs.Add((child as EditionItem).Data);
            }
            return editionConfigs;
        }

        public void SetData(BuildConfig config)
        {
            if (abRootItem.hasChildren)
            {
                abRootItem.children.Clear();
            }

            config ??= new BuildConfig();

            int childDepth = abRootItem.depth + 1;
            foreach (EditionConfig data in config.editionConfigs)
            {
                EditionItem item = new EditionItem(data, this)
                {
                    depth = childDepth
                };
                abRootItem.AddChild(item);
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
            (args.item as EditionItem).OnGui(args.rowRect);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return (item as EditionItem) != null;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (FindItem(args.itemID, abRootItem) is EditionItem item)
            {
                buildEditor.OnEditionItemRename(item, args.newName);
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
            {
                buildEditor.OnEditionItemSelected(null);
                return;
            }

            EditionItem item = FindItem(selectedIds[0], abRootItem) as EditionItem;
            buildEditor.OnEditionItemSelected(item);
        }

        public void AddEditionItem(EditionConfig data)
        {
            EditionItem item = new EditionItem(data, this)
            {
                depth = abRootItem.depth + 1
            };
            abRootItem.AddChild(item);
        }
    }
}