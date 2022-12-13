using System;
using System.Collections.Generic;

namespace Morpheus.Ecs
{
    public interface IComponentConfig
    {
        Type type {get;}
        void Apply(EcsComponent c);

        IComponentConfig Combine(IComponentConfig b);
        
        public static IComponentConfig operator +(IComponentConfig c1, IComponentConfig c2)
        {
            return c1.Combine(c2);
        }
    }
    
    public struct ComponentConfig<T> : IComponentConfig where T : EcsComponent 
    {
        public static ComponentConfig<T> operator +(ComponentConfig<T> c1, ComponentConfig<T> c2)
        {
            return (ComponentConfig<T>)(c1.Combine(c2));
        }

        public IComponentConfig Combine(IComponentConfig c2)
        {
            ApplyAction += ((ComponentConfig<T>)c2).ApplyAction;
            return this;
        }

        public Type type => typeof(T);
        private Action<T> ApplyAction;
        
        public ComponentConfig(Action<T> _applyAction = null)
        {
            ApplyAction = _applyAction;
        }

        public void Apply(EcsComponent _c)
        {
            ApplyAction?.Invoke((T)_c);
        }
    }
}