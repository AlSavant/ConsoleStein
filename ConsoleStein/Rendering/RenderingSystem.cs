using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ConsoleStein.Input;
using ConsoleStein.Time;
using ConsoleStein.Maths;
using System.Collections.Generic;
using System.Linq;

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
        private string DisplayMap { get; set; }

        public short ScreenWidth { get; set; }
        public short ScreenHeight { get; set; }

        private float FOV { get; set; } = 3.14159f / 4f;
        private float Depth { get; set; } = 16f;
        private Transform Player { get; set; }

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

            DisplayMap = string.Empty;
            DisplayMap += "###~LEVEL-1~####";
            DisplayMap += "#              #";
            DisplayMap += "#              #";
            DisplayMap += "#              #";
            DisplayMap += "#          #####";
            DisplayMap += "#              #";
            DisplayMap += "#              #";
            DisplayMap += "#####          #";
            DisplayMap += "#              #";
            DisplayMap += "#              #";
            DisplayMap += "################";
            ScreenWidth = (short)Console.WindowWidth;
            ScreenHeight = (short)Console.WindowHeight;
            Player = new Transform();
            Player.position = new Vector2(8f, 5f);
            UpdateCharBuffer();
        }

        public void Update()
        {
            if (Handle.IsInvalid)
                return;

            if(inputSystem.GetKey(EKeyCode.A))
            {
                Player.rotation -= 0.5f * TimeSystem.DeltaTime;
            }
            if(inputSystem.GetKey(EKeyCode.D))
            {
                Player.rotation += 0.5f * TimeSystem.DeltaTime;
            }

            if(inputSystem.GetKey(EKeyCode.W))
            {
                Player.Translate(Player.forward, TimeSystem.DeltaTime);                

                if(Map[(int)Player.position.y * 16 + (int)Player.position.x] != '.')
                {
                    Player.Translate(Player.back, TimeSystem.DeltaTime);                    
                }
            }

            if (inputSystem.GetKey(EKeyCode.S))
            {
                Player.Translate(Player.back, TimeSystem.DeltaTime);                

                if (Map[(int)Player.position.y * 16 + (int)Player.position.x] != '.')
                {
                    Player.Translate(Player.forward, TimeSystem.DeltaTime);                    
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
                float angle = Player.rotation - FOV / 2f + x / (float)ScreenWidth * FOV;
                float dist = 0f;
                bool hitWall = false;
                bool hitBoundary = false;
                short wallColor = 0;

                Vector2 eyeVec = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));                
                while(!hitWall && dist < Depth)
                {
                    dist += 0.1f;

                    int testX = (int)(Player.position.x + eyeVec.x * dist);
                    int testY = (int)(Player.position.y + eyeVec.y * dist);

                    if(testX < 0 || testX >= 16 || testY < 0 || testY >= 11)
                    {
                        hitWall = true;
                        dist = Depth;
                    }
                    else if(Map[testY * 16 + testX] != '.')
                    {
                        hitWall = true;

                        List<Vector2> p = new List<Vector2>();
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                Vector2 vec = new Vector2
                                    (
                                        (float)testX + j - Player.position.x,
                                        (float)testY + k - Player.position.y
                                    );
                                float magnitude = vec.magnitude;
                                float dot = Vector2.Dot(eyeVec, vec);
                                p.Add(new Vector2(magnitude, dot));
                            }
                        }
                        p = p.OrderBy(v => v.x).ToList();

                        float bound = 0.005f;
                        if ((float)Math.Acos(p[0].y) < bound)
                            hitBoundary = true;
                        if ((float)Math.Acos(p[1].y) < bound)
                            hitBoundary = true;
                        if ((float)Math.Acos(p[2].y) < bound)
                            hitBoundary = true;

                        wallColor = GetWallColor(Map[testY * 16 + testX]);
                    }
                }

                int ceiling = (int)((ScreenHeight / 2f) - ScreenHeight / (dist));
                int floor = ScreenHeight - ceiling;

                byte wallShade = GetWallShade(dist);

                if (hitBoundary)
                    wallShade = (byte)' ';

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
                        CharBuffer[index].Char.UnicodeChar = (char)wallShade;
                    }
                    else
                    {
                        CharBuffer[index].Attributes = 2;
                        CharBuffer[index].Char.AsciiChar = floorShade;
                    }
                }
            }
            PrintText(new RectInt(0, 0, 16, 11), DisplayMap, ConsoleColor.Cyan);
            PrintText(new RectInt((int)Player.position.x, 10 - (int)Player.position.y,1,1), ((char)GetRotationChar(Player.rotation)).ToString(), ConsoleColor.Yellow);

            WriteConsoleOutput(Handle, CharBuffer,
                     new Coord() { X = width, Y = height },
                     new Coord() { X = 0, Y = 0 },
                     ref bufferRect);            
        }  
        
        public void PrintText(RectInt rect, string text, ConsoleColor color, bool blackIsTransparency = false)
        {
            for (int x = 0; x < rect.width; x++)
            {
                for (int y = 0; y < rect.height; y++)
                {
                    int rectIndex = y * 16 + x;
                    int screenIndex = (rect.y + y) * ScreenWidth + rect.x + x;
                    if (text[rectIndex] == ' ' && blackIsTransparency)
                        continue;
                    CharBuffer[screenIndex].Attributes = (short)color;
                    CharBuffer[screenIndex].Char.UnicodeChar = text[rectIndex];
                    CharBuffer[screenIndex].Char.AsciiChar = (byte)text[rectIndex];
                }
            }
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

        private byte GetRotationChar(float rotation)
        {
            float angle = rotation * (180f / (float)Math.PI);
            angle %= 360;
            if (angle < 0)
                angle += 360;
            int snap = (int)(Math.Round(angle / 45f) * 45f);
            switch(snap)
            {
                case 0:
                case 360:
                    return 94;

                case 45:
                    return 191;                    

                case 90:

                    return 62;

                case 135:
                    return 217;

                case 180:
                    return 118;

                case 225:
                    return 192;

                case 270:
                    return 60;

                case 315:
                    return 218;
            }
            return 94;
        }

        private short GetWallColor(char code)
        {
            
            return (short)((byte)code - 65);
        }
    }    
}
