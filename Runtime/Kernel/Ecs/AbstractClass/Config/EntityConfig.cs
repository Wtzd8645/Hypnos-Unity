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
            foreach(var c in c2.componentConfigMap)
            {
                if (!c1.componentConfigMap.TryGetValue(c.Key, out var config))
                {
                    c1.componentConfigMap[c.Key] = c.Value;
                }
                else
                {
                    config += c.Value;
                }
            }
            return c1;
        }

        Dictionary<Type, IComponentConfig> componentConfigMap;

        public IEnumerable<IComponentConfig> ComponentTypes => componentConfigMap.Values;

        public EntityConfig(params IComponentConfig[] configs)
        {
            componentConfigMap = new Dictionary<Type, IComponentConfig>();
            foreach(var c in configs)
            {
                componentConfigMap.Add(c.Type, c);
            }
        }

        public void AddComponentConfig(IComponentConfig _config)
        {
            if (!componentConfigMap.TryGetValue(_config.Type, out var config))
            {
                componentConfigMap[_config.Type] = _config;
            }
            else
            {
                config += _config;
            }
        }
    }
}