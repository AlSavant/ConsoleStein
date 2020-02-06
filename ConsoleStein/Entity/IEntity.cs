using System;
using System.Collections.Generic;
using ConsoleStein.Components;    

namespace ConsoleStein
{
    public interface IEntity
    {
        Dictionary<Type, IComponent> Components { get; set; }

        void AddComponent(Type type, IComponent component);
        T GetComponent<T>() where T : IComponent;
        object GetFirstComponentOfType(Type type);        
    }
}
