using System;
using ConsoleStein.Rendering;
using ConsoleStein.Input;
using ConsoleStein.Time;
using ConsoleStein.Resources;

namespace ConsoleStein
{
    class ConsoleStein
    {        
        [STAThread]
        static void Main(string[] args)
        {
            var resourcesSystem = new ResourcesSystem();
            var renderingSystem = new RenderingSystem();
            var inputSystem = new InputSystem();
            var timeSystem = new TimeSystem();
            resourcesSystem.Setup();
            timeSystem.Setup();
            inputSystem.Setup();
            renderingSystem.Setup(inputSystem, resourcesSystem);            
            while(true)
            {
                timeSystem.Update();
                renderingSystem.Update();
                inputSystem.Update();
            }
        }
    }
}
