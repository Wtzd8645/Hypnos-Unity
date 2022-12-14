using Morpheus.Core.Memory;
using System;
using System.Collections.Generic;

namespace Morpheus.Ecs
{
    public partial class NodeManager
    {
        private Dictionary<Type, ObjectPool<EcsNode>> nodePools = new Dictionary<Type, ObjectPool<EcsNode>>();
        private Dictionary<ulong, Dictionary<Type, EcsNode>> entityNodeDict = new Dictionary<ulong, Dictionary<Type, EcsNode>>();

        private void AddNode2System(Type type, ulong entityId)
        {
            EcsNode node = GetNode(type);
            node.Init(entityId);
            SystemManager.Instance.AddNode(node);

            if (!entityNodeDict.TryGetValue(entityId, out Dictionary<Type, EcsNode> nodeDict))
            {
                entityNodeDict[entityId] = nodeDict = new Dictionary<Type, EcsNode>();
            }
            nodeDict.Add(type, node);

            DebugLogger.TraceLog($"Entity {entityId} Add Node {type}");
        }

        private void RemoveNodeFromSystem(Type type, ulong entityId)
        {
            if (!entityNodeDict.TryGetValue(entityId, out Dictionary<Type, EcsNode> nodeDict))
            {
                throw new Exception($"entityNodeDict doesn't contain the Entity {entityId}.");
            }

            if (!nodeDict.TryGetValue(type, out EcsNode node))
            {
                throw new Exception($"Entity {entityId} doesn't have the Type {type}.");
            }
            SystemManager.Instance.RemoveNode(node);
            node.Dispose();
            RecycleNode(type, node);

            nodeDict.Remove(type);
            if (nodeDict.Count == 0)
            {
                entityNodeDict.Remove(entityId);
            }
            DebugLogger.TraceLog($"Entity {entityId} Remove Node {type}");
        }

        private EcsNode GetNode(Type type)
        {
            if (!nodePools.TryGetValue(type, out ObjectPool<EcsNode> objectPool))
            {
                objectPool = new ObjectPool<EcsNode>(() => (EcsNode)Activator.CreateInstance(type));
                nodePools[type] = objectPool;
            }
            return objectPool.ForcePop();
        }

        private void RecycleNode(Type type, EcsNode node)
        {
            if (nodePools.TryGetValue(type, out ObjectPool<EcsNode> objectPool))
            {
                objectPool.ForcePush(node);
            }
            else
            {
                throw new Exception($"Can't find Node object pool of Type {type}.");
            }
        }
    }
}