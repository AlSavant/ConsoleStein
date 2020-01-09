using System;
using ConsoleStein.Rendering;
using ConsoleStein.Input;
using ConsoleStein.Time;

namespace ConsoleStein
{
    class ConsoleStein
    {        
        [STAThread]
        static void Main(string[] args)
        {
            var renderingSystem = new RenderingSystem();
            var inputSystem = new InputSystem();
            var timeSystem = new TimeSystem();
            timeSystem.Setup();
            inputSystem.Setup();
            renderingSystem.Setup(inputSystem);            
            while(true)
            {
                timeSystem.Update();
                renderingSystem.Update();
                inputSystem.Update();
            }
        }
    }
}
