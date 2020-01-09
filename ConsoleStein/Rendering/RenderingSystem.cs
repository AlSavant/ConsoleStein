using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ConsoleStein.Input;
using ConsoleStein.Time;

namespace ConsoleStein.Rendering
{
    internal sealed class RenderingSystem
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        private SafeFileHandle Handle { get; set; }
        private CharInfo[] CharBuffer { get; set; }
        private SmallRect bufferRect;
        private string Map { get; set; }

        public short ScreenWidth { get; set; }
        public short ScreenHeight { get; set; }

        private float FOV { get; set; } = 3.14159f / 4f;
        private float Depth { get; set; } = 16f;
        private float Angle { get; set; }
        private float PlayerX = 8f;
        private float PlayerY = 5f;

        private InputSystem inputSystem;

        public void Setup(InputSystem inputSystem)
        {
            this.inputSystem = inputSystem;
            Handle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            Map = string.Empty;
            Map += "MMMMMMMMMMMMMMMM";
            Map += "M..............M";
            Map += "M..............M";
            Map += "MFFFF..........M";
            Map += "M..............M";
            Map += "M..............M";
            Map += "M..........OOOOM";
            Map += "M..............M";
            Map += "M..............M";
            Map += "M..............M";
            Map += "MMMMMMMMMMMMMMMM";
            ScreenWidth = (short)Console.WindowWidth;
            ScreenHeight = (short)Console.WindowHeight;
            UpdateCharBuffer();
        }

        public void Update()
        {
            if (Handle.IsInvalid)
                return;

            if(inputSystem.GetKey(EKeyCode.A))
            {
                Angle -= 0.5f * TimeSystem.DeltaTime;
            }
            if(inputSystem.GetKey(EKeyCode.D))
            {
                Angle += 0.5f * TimeSystem.DeltaTime;
            }

            if(inputSystem.GetKey(EKeyCode.W))
            {
                PlayerX += (float)Math.Sin(Angle) * TimeSystem.DeltaTime;
                PlayerY += (float)Math.Cos(Angle) * TimeSystem.DeltaTime;

                if(Map[(int)PlayerY * 16 + (int)PlayerX] != '.')
                {
                    PlayerX -= (float)Math.Sin(Angle) * TimeSystem.DeltaTime;
                    PlayerY -= (float)Math.Cos(Angle) * TimeSystem.DeltaTime;
                }
            }

            if (inputSystem.GetKey(EKeyCode.S))
            {
                PlayerX -= (float)Math.Sin(Angle) * TimeSystem.DeltaTime;
                PlayerY -= (float)Math.Cos(Angle) * TimeSystem.DeltaTime;

                if (Map[(int)PlayerY * 16 + (int)PlayerX] != '.')
                {
                    PlayerX += (float)Math.Sin(Angle) * TimeSystem.DeltaTime;
                    PlayerY += (float)Math.Cos(Angle) * TimeSystem.DeltaTime;
                }
            }

            var width = (short)Console.WindowWidth;
            var height = (short)Console.WindowHeight;
            if(width != ScreenWidth || height != ScreenHeight)
            {
                ScreenWidth = width;
                ScreenHeight = height;
                UpdateCharBuffer();
            } 
            
            for(short x = 0; x < ScreenWidth; x++)
            {
                float angle = Angle - FOV / 2f + x / (float)ScreenWidth * FOV;
                float dist = 0f;
                bool hitWall = false;
                short wallColor = 0;

                float eyeX = (float)Math.Sin(angle);
                float eyeY = (float)Math.Cos(angle);
                while(!hitWall && dist < Depth)
                {
                    dist += 0.1f;

                    int testX = (int)(PlayerX + eyeX * dist);
                    int testY = (int)(PlayerY + eyeY * dist);

                    if(testX < 0 || testX >= 16 || testY < 0 || testY >= 11)
                    {
                        hitWall = true;
                        dist = Depth;
                    }
                    else
                    {
                        if(Map[testY * 16 + testX] != '.')
                        {
                            hitWall = true;
                            wallColor = GetWallColor(Map[testY * 16 + testX]);
                        }
                    }
                }

                int ceiling = (int)((ScreenHeight / 2f) - ScreenHeight / (dist));
                int floor = ScreenHeight - ceiling;

                byte wallShade = GetWallShade(dist);                

                for(int y = 0; y < ScreenHeight; y++)
                {
                    byte floorShade = GetFloorShade(y);
                    int index = y * ScreenWidth + x;
                    if(y <= ceiling)
                    {
                        CharBuffer[index].Attributes = 1;
                        CharBuffer[index].Char.AsciiChar = (byte)'*';
                    }
                    else if(y > ceiling && y <= floor)
                    {
                        CharBuffer[index].Attributes = wallColor;
                        CharBuffer[index].Char.AsciiChar = wallShade;
                    }
                    else
                    {
                        CharBuffer[index].Attributes = 2;
                        CharBuffer[index].Char.AsciiChar = floorShade;
                    }
                }
            }

            WriteConsoleOutput(Handle, CharBuffer,
                     new Coord() { X = width, Y = height },
                     new Coord() { X = 0, Y = 0 },
                     ref bufferRect);            
        }

        private void UpdateCharBuffer()
        {
            CharBuffer = new CharInfo[ScreenWidth * ScreenHeight];
            bufferRect = new SmallRect() { Left = 0, Top = 0, Right = ScreenWidth, Bottom = ScreenHeight };
        }

        private byte GetWallShade(float dist)
        {
            if(dist <= Depth / 4f)
            {
                return 219; 
            }
            if(dist < Depth / 3f)
            {
                return 178;
            }
            if(dist < Depth / 2f)
            {
                return 177;
            }
            if(dist < Depth)
            {
                return 176;
            }
            return (byte)' ';
            
        }

        private byte GetFloorShade(int y)
        {
            float b = 1f - ((y - ScreenHeight / 2f) / (ScreenHeight / 2f));
            if (b < 0.25f)
            {
                return (byte)'#';
            }
            if (b < 0.5f)
            {
                return (byte)'x';
            }
            if (b < 0.75f)
            {
                return (byte)'-';
            }
            if (b < 0.9f)
            {
                return (byte)'.';
            }
            return (byte)' ';

        }

        private short GetWallColor(char code)
        {
            
            return (short)((byte)code - 65);
        }
    }    
}
