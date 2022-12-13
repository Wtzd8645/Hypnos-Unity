using System;
using System.Collections.Generic;

namespace Morpheus.Ecs
{
    public interface IEntityConfig
    {
        IEnumerable<IComponentConfig> ComponentTypes {get;}
    }

    public struct EntityConfig : IEntityConfig
    {
        public static EntityConfig operator +(EntityConfig c1, EntityConfig c2)
        {
            foreach(var c in c2.componenetConfigMap)
            {
                if (!c1.componenetConfigMap.TryGetValue(c.Key, out var config))
                {
                    c1.componenetConfigMap[c.Key] = c.Value;
                }
                else
                {
                    config += c.Value;
                }
            }
            return c1;
        }

        Dictionary<Type, IComponentConfig> componenetConfigMap;

        public IEnumerable<IComponentConfig> ComponentTypes => componenetConfigMap.Values;

        public EntityConfig(params IComponentConfig[] configs)
        {
            componenetConfigMap = new Dictionary<Type, IComponentConfig>();
            foreach(var c in configs)
            {
                componenetConfigMap.Add(c.type, c);
            }
        }

        public void AddComponentConfig(IComponentConfig _config)
        {
            if (!componenetConfigMap.TryGetValue(_config.type, out var config))
            {
                componenetConfigMap[_config.type] = _config;
            }
            else
            {
                config += _config;
            }
        }
    }
}