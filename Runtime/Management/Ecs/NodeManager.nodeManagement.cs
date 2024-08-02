using Blanketmen.Hypnos.Cache;
using System;
using System.Collections.Generic;

namespace Blanketmen.Hypnos
{
    public partial class NodeManager
    {
        private Dictionary<Type, ObjectPool<EcsNode>> nodePools = new Dictionary<Type, ObjectPool<EcsNode>>();
        private Dictionary<ulong, Dictionary<Type, EcsNode>> entityNodeDict = new Dictionary<ulong, Dictionary<Type, EcsNode>>();

        private void AddNode2System(Type type, ulong entityID)
        {
            EcsNode node = GetNode(type);
            node.Init(entityID);
            SystemManager.Instance.AddNode(node);

            if (!entityNodeDict.TryGetValue(entityID, out Dictionary<Type, EcsNode> nodeDict))
            {
                entityNodeDict[entityID] = nodeDict = new Dictionary<Type, EcsNode>();
            }
            nodeDict.Add(type, node);
        }

        private void RemoveNodeFromSystem(Type type, ulong entityID)
        {
            if (!entityNodeDict.TryGetValue(entityID, out Dictionary<Type, EcsNode> nodeDict))
            {
                throw new Exception($"entityNodeDict doesn't contain the Entity {entityID}.");
            }

            if (!nodeDict.TryGetValue(type, out EcsNode node))
            {
                throw new Exception($"Entity {entityID} doesn't have the Type {type}.");
            }

            node.Dispose();
            SystemManager.Instance.RemoveNode(node);
            RecycleNode(type, node);

            nodeDict.Remove(type);
            if (nodeDict.Count == 0)
            {
                entityNodeDict.Remove(entityID);
            }
        }

        private EcsNode GetNode(Type type)
        {
            if (!nodePools.TryGetValue(type, out ObjectPool<EcsNode> objectPool))
            {
                objectPool = new ObjectPool<EcsNode>(() => (EcsNode)Activator.CreateInstance(type));
                nodePools[type] = objectPool;
            }
            return objectPool.Pop();
        }

        private void RecycleNode(Type type, EcsNode node)
        {
            if (nodePools.TryGetValue(type, out ObjectPool<EcsNode> objectPool))
            {
                objectPool.Push(node);
            }
            else
            {
                throw new Exception($"Can't find Node object pool of Type {type}.");
            }
        }
    }
}