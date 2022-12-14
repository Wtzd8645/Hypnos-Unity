using Morpheus.Core.Collection;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Morpheus.Ecs
{
    public partial class NodeManager
    {
        private ComponentTypeInNodeDic componentTypeDic;
        private OrderedDictionary<Type, HashSet<Type>> nodeToComponentDict = new OrderedDictionary<Type, HashSet<Type>>();
        private Dictionary<Type, BigInteger> nodeHashes = new Dictionary<Type, BigInteger>();
        private Dictionary<Type, BigInteger> componentHashes = new Dictionary<Type, BigInteger>();
        private Dictionary<ulong, BigInteger> entityHashes = new Dictionary<ulong, BigInteger>();

        public NodeManager()
        {
            componentTypeDic = ComponentTypeInNodeDic.GetInstanceByAssembly();
            RefreshMaps();
        }

        private void RefreshMaps()
        {
            nodeToComponentDict.Clear();
            componentHashes.Clear();
            nodeHashes.Clear();

            foreach (KeyValuePair<Type, HashSet<Type>> eseentials in componentTypeDic)
            {
                foreach (Type com in eseentials.Value)
                {
                    if (!nodeToComponentDict.TryGetValue(com, out HashSet<Type> nodes))
                    {
                        nodes = new HashSet<Type>();
                        nodeToComponentDict.Add(com, nodes);
                    }
                    nodes.Add(eseentials.Key);
                }
            }

            BigInteger i = 1;
            foreach (KeyValuePair<Type, HashSet<Type>> comPair in nodeToComponentDict)
            {
                componentHashes.Add(comPair.Key, i);
                i <<= 1;
            }

            foreach (KeyValuePair<Type, HashSet<Type>> eseentials in componentTypeDic)
            {
                nodeHashes.Add(eseentials.Key, 0);
                foreach (Type com in eseentials.Value)
                {
                    nodeHashes[eseentials.Key] += componentHashes[com];
                }
            }
        }

        public void AddComponentSet(ComponentTypeInNodeDic set)
        {
            componentTypeDic += set;
            RefreshMaps();
        }

        public void OnComponentAdd(EcsComponent component)
        {
            if (!entityHashes.ContainsKey(component.EntityId))
            {
                entityHashes.Add(component.EntityId, 0);
            }
            
            if (!componentHashes.TryGetValue(component.GetType(), out var componentHash)) return;

            BigInteger oldValue = entityHashes[component.EntityId];
            BigInteger newValue = entityHashes[component.EntityId] |= componentHash;

            foreach (Type nodeType in nodeToComponentDict[component.GetType()])
            {
                if ((nodeHashes[nodeType] & oldValue) != nodeHashes[nodeType]
                    && (nodeHashes[nodeType] & newValue) == nodeHashes[nodeType])
                {
                    AddNode2System(nodeType, component.EntityId);
                }
            }
        }

        public void OnComponentRemove(EcsComponent component)
        {
            if (!entityHashes.ContainsKey(component.EntityId))
            {
                throw new Exception($"Entity {component.EntityId} doesn't exist.");
            }

            // Could be optional component.
            if (!componentHashes.TryGetValue(component.GetType(), out var componentHash)) return;

            BigInteger oldValue = entityHashes[component.EntityId];
            BigInteger newValue = entityHashes[component.EntityId] &= ~componentHash;

            foreach (Type nodeType in nodeToComponentDict[component.GetType()])
            {
                if ((nodeHashes[nodeType] & oldValue) == nodeHashes[nodeType]
                    && (nodeHashes[nodeType] & newValue) != nodeHashes[nodeType])
                {
                    RemoveNodeFromSystem(nodeType, component.EntityId);
                }
            }
        }
    }
}