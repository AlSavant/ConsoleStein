using System;
using ConsoleStein.Rendering;
using ConsoleStein.Input;

namespace ConsoleStein
{
    class ConsoleStein
    {        
        [STAThread]
        static void Main(string[] args)
        {
            var renderingSystem = new RenderingSystem();
            renderingSystem.Setup();
            while(true)
            {
                if(InputSystem.GetKey(ConsoleKey.D))
                {
                    Console.WriteLine("D");
                }
                if(InputSystem.GetKey(ConsoleKey.A))
                {
                    Console.WriteLine("A");
                }
                renderingSystem.Update();
            }
        }
    }
}
