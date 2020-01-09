using System.Collections.Generic;
using System.Windows.Input;
using System;

namespace ConsoleStein.Input
{
    public sealed class InputSystem
    {
        private HashSet<EKeyCode> keyPresses;
        private HashSet<EKeyCode> keyDowns;
        private EKeyCode[] keyCodes;

        public bool GetKey(EKeyCode key)
        {            
            return Keyboard.IsKeyDown((Key)key);
        }

        public bool GetKeyDown(EKeyCode key)
        {
            if (Keyboard.IsKeyDown((Key)key))
            {
                bool justPressed = false;
                if (!keyDowns.Contains(key))
                {
                    justPressed = true;
                    keyDowns.Add(key);
                }
                return justPressed;
            }
            return false;
        }

        public bool GetKeyUp(EKeyCode key)
        {
            if(Keyboard.IsKeyUp((Key)key))
            {
                if(keyPresses.Contains(key))
                {
                    keyPresses.Remove(key);
                    return true;
                }
                
            }
            return false;
        }

        public void Setup()
        {
            keyPresses = new HashSet<EKeyCode>();
            keyDowns = new HashSet<EKeyCode>();   
            keyCodes = (EKeyCode[])Enum.GetValues(typeof(EKeyCode));
        }
        
        public void Update()
        {
            foreach(var key in keyCodes)
            {
                var code = (Key)key;
                if (code == Key.None)
                    continue;
                if(Keyboard.IsKeyDown(code))
                {
                    keyPresses.Add(key);
                }

                if(Keyboard.IsKeyUp(code))
                {
                    keyDowns.Remove(key);
                }
            }
        }
    }
}
