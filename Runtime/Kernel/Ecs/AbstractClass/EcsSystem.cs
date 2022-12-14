using System;

namespace Morpheus.Ecs
{
    public abstract class EcsSystem
    {
        public abstract void Update();
        public abstract void AddNode(object _node);
        public abstract void RemoveNode(object _node);
    }

    /// Node系統是個環型系統，為了方便增減
    public abstract class EcsSystem<T> : EcsSystem where T : EcsNode
    {
        private T headNode;

        public override void Update()
        {
            if (headNode != null)
            {
                UpdateNode(headNode);
            }
        }

        public override void AddNode(object _node)
        {
            T node = (T)_node;

            if (headNode == null)
            {
                headNode = node;
                headNode.PrevNode = headNode.NextNode = headNode;
            }
            else if (headNode.PrevNode == headNode)
            {
                // if there's no headNode.LastNode
                headNode.PrevNode = headNode.NextNode = node;
                node.PrevNode = node.NextNode = headNode;
            }
            else
            {
                // Add to last
                node.PrevNode = headNode.PrevNode;
                node.NextNode = headNode;

                headNode.PrevNode.NextNode = node;
                headNode.PrevNode = node;
            }
        }

        public override void RemoveNode(object _node)
        {
            T node = (T)_node;

            // if node.LastNode is null, which means it's the only one.
            // And it's the headNode.
            if (node == headNode)
            {
                if (node.NextNode == headNode)
                {
                    headNode = null;
                    return;
                }
                else
                {
                    headNode = (T)node.NextNode;
                }
            }

            node.PrevNode.NextNode = node.NextNode;
            node.NextNode.PrevNode = node.PrevNode;
        }

        private void UpdateNode(T node)
        {
            Process(node);

            if (node.NextNode != headNode)
            {
                UpdateNode((T)node.NextNode);
            }
        }

        protected abstract void Process(T node);
    }
}