using System;
using System.Collections.Generic;
using ConsoleStein.Components;

namespace ConsoleStein
{
    public sealed class Entity : IEntity
    {
        public Dictionary<Type, IComponent> Components { get; set; }

        public Entity()
        {
            Components = new Dictionary<Type, IComponent>();            
        }

        public void AddComponent(Type type, IComponent component)
        {
            Components.Add(type, component);
            component.Entity = this;
        }

        public T GetComponent<T>() where T : IComponent
        {
            return (T)Components[typeof(T)];
        }

        public object GetFirstComponentOfType(Type type)
        {
            foreach (var component in Components)
            {
                var componentType = component.Value.GetType();
                if (componentType.IsAssignableFrom(type) ||
                    type.IsAssignableFrom(componentType))
                    return component.Value;
            }
            return null;
        }
    }
}