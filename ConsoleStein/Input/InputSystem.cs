using System;

namespace ConsoleStein.Input
{
    public sealed class InputSystem
    {
        public static bool GetKey(ConsoleKey key)
        {
            if (!Console.KeyAvailable)
                return false;
            if (Console.ReadKey(true).Key == key)
                return true;
            return false;
        }
    }
}
