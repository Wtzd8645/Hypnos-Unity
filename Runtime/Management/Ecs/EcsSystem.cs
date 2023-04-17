using System;

namespace Blanketmen.Hypnos
{
    public abstract class EcsSystem
    {
        public abstract void Update();
        public abstract void AddNode(object _node);
        public abstract void RemoveNode(object _node);
    }

    public abstract class EcsSystem<T> : EcsSystem where T : EcsNode
    {
        private T headNode;

        public override void Update()
        {
            updateNode(headNode);
        }

        public override void AddNode(object _node)
        {
            T node = (T)_node;

            if (headNode == null)
            {
                headNode = node;
            }
            else if (headNode.LastNode == null)
            {
                // if there's no headNode.LastNode
                headNode.LastNode = headNode.NextNode = node;
            }
            else
            {
                // Add to last
                node.LastNode = headNode.LastNode;
                headNode.LastNode.NextNode = node;
                headNode.LastNode = node;
            }
        }

        public override void RemoveNode(object _node)
        {
            T node = (T)_node;

            // if node.LastNode is null, which means it's the only one.
            // And it's the headNode.
            if (node.LastNode == null)
            {
                if (node != headNode)
                {
                    throw new Exception($"Node has no lastNode and isn't the headNode.");
                }
                headNode = null;
            }
            else
            {
                node.LastNode.NextNode = node.NextNode;
            }
        }

        private void updateNode(T node)
        {
            if (node == null)
            {
                return;
            }

            Process(node);
            updateNode((T)node.NextNode);
        }

        protected abstract void Process(T node);
    }
}