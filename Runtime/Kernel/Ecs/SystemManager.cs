using Morpheus.Core.Collection;
using System;
using System.Collections.Generic;

namespace Morpheus.Ecs
{
    public struct SystemConfig
    {
        public Type[] UpdateSystems;
        public Type[] LateUpdateSystems;
        public Type[] FixedUpdateSystems;

        public SystemConfig(Type[] updateSystems = null, Type[] lateUpdateSystems = null, Type[] fixedUpdateSystems = null)
        {
            UpdateSystems = updateSystems;
            LateUpdateSystems = lateUpdateSystems;
            FixedUpdateSystems = fixedUpdateSystems;
        }
    }

    public class SystemManager : Singleton<SystemManager>
    {
        private Dictionary<Type, EcsSystem> systemWithBelongNodeDict = new Dictionary<Type, EcsSystem>();
        private OrderedSet<EcsSystem> updateSystems = new OrderedSet<EcsSystem>();
        private OrderedSet<EcsSystem> lateUpdateSystems = new OrderedSet<EcsSystem>();
        private OrderedSet<EcsSystem> fixedUpdateSystems = new OrderedSet<EcsSystem>();

        public void Init(SystemConfig config)
        {
            void setSystems(Type[] systemTypes, OrderedSet<EcsSystem> systems)
            {
                if (systemTypes == null || systemTypes.Length == 0)
                {
                    return;
                }

                systems.Clear();
                foreach (Type type in systemTypes)
                {
                    Type[] genericArguments = type.BaseType.GenericTypeArguments;
                    if (genericArguments != Type.EmptyTypes)
                    {
                        if (systemWithBelongNodeDict.ContainsKey(genericArguments[0]))
                        {
                            throw new Exception($"Duplicated system {type} add to the Manager.Need to check the config.");
                        }

                        EcsSystem system = (EcsSystem)Activator.CreateInstance(type);
                        systemWithBelongNodeDict.Add(
                            genericArguments[0],
                            system);
                        systems.Add(system);
                    }
                    else
                    {
                        throw new Exception($"System type {type} don't have node genericArguments");
                    }
                }
            }

            systemWithBelongNodeDict.Clear();
            setSystems(config.UpdateSystems, updateSystems);
            setSystems(config.LateUpdateSystems, lateUpdateSystems);
            setSystems(config.FixedUpdateSystems, fixedUpdateSystems);
        }

        public void Update()
        {
            foreach (EcsSystem system in updateSystems)
            {
                system.Update();
            }
        }

        public void FixedUpdate()
        {
            foreach (EcsSystem system in fixedUpdateSystems)
            {
                system.Update();
            }
        }

        public void LateUpdate()
        {
            foreach (EcsSystem system in lateUpdateSystems)
            {
                system.Update();
            }
        }

        public void AddNode(EcsNode node)
        {
            Type nodeType = node.GetType();
            if (systemWithBelongNodeDict.ContainsKey(nodeType))
            {
                systemWithBelongNodeDict[nodeType].AddNode(node);
            }
        }

        public void RemoveNode(EcsNode node)
        {
            Type nodeType = node.GetType();
            if (systemWithBelongNodeDict.ContainsKey(nodeType))
            {
                systemWithBelongNodeDict[nodeType].RemoveNode(node);
            }
        }
    }
}