using Morpheus.Core.Collection;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Morpheus.Ecs
{
    public partial class NodeManager
    {
        private ComonentTypeInNodeDic componentTypeDic;
        private OrderedDictionary<Type, HashSet<Type>> nodeToComponentDict = new OrderedDictionary<Type, HashSet<Type>>();
        private Dictionary<Type, BigInteger> nodeHashes = new Dictionary<Type, BigInteger>();
        private Dictionary<Type, BigInteger> componentHashes = new Dictionary<Type, BigInteger>();
        private Dictionary<ulong, BigInteger> entityHashes = new Dictionary<ulong, BigInteger>();

        public NodeManager()
        {
            componentTypeDic = ComonentTypeInNodeDic.GetInstanceByAssembly();
            refreshMaps();
        }

        private void refreshMaps()
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

        public void AddComponentSet(ComonentTypeInNodeDic set)
        {
            componentTypeDic += set;
            refreshMaps();
        }

        public void OnComponentAdd(EcsComponent component)
        {
            if (!entityHashes.ContainsKey(component.EntityID))
            {
                entityHashes.Add(component.EntityID, 0);
            }
            
            if (!componentHashes.TryGetValue(component.GetType(), out var componentHash)) return;

            BigInteger oldValue = entityHashes[component.EntityID];
            BigInteger newValue = entityHashes[component.EntityID] |= componentHash;

            foreach (Type nodeType in nodeToComponentDict[component.GetType()])
            {
                if ((nodeHashes[nodeType] & oldValue) != nodeHashes[nodeType]
                    && (nodeHashes[nodeType] & newValue) == nodeHashes[nodeType])
                {
                    AddNode2System(nodeType, component.EntityID);
                }
            }
        }

        public void OnComponentRemove(EcsComponent component)
        {
            if (!entityHashes.ContainsKey(component.EntityID))
            {
                throw new Exception($"Entity {component.EntityID} doesn't exist.");
            }

            // Could be optional component.
            if (!componentHashes.TryGetValue(component.GetType(), out var componentHash)) return;

            BigInteger oldValue = entityHashes[component.EntityID];
            BigInteger newValue = entityHashes[component.EntityID] &= ~componentHash;

            foreach (Type nodeType in nodeToComponentDict[component.GetType()])
            {
                if ((nodeHashes[nodeType] & oldValue) == nodeHashes[nodeType]
                    && (nodeHashes[nodeType] & newValue) != nodeHashes[nodeType])
                {
                    RemoveNodeFromSystem(nodeType, component.EntityID);
                }
            }
        }
    }
}