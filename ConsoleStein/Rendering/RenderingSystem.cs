using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

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

        public void Setup()
        {
            Handle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            Map = string.Empty;
            Map += "################";
            Map += "#..............#";
            Map += "#..............#";
            Map += "#..............#";
            Map += "#..............#";
            Map += "#..............#";
            Map += "#..............#";
            Map += "#..............#";
            Map += "#..............#";
            Map += "#..............#";
            Map += "################";
            ScreenWidth = (short)Console.WindowWidth;
            ScreenHeight = (short)Console.WindowHeight;
            UpdateCharBuffer();
        }

        public void Update()
        {
            if (Handle.IsInvalid)
                return;

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
                        if(Map[testY * 16 + testX] == '#')
                        {
                            hitWall = true;
                        }
                    }
                }

                int ceiling = (int)((float)(ScreenHeight / 2f) - ScreenHeight / ((float)dist));
                int floor = ScreenHeight - ceiling;

                for(int y = 0; y < ScreenHeight; y++)
                {
                    int index = y * ScreenWidth + x;
                    if(y < ceiling)
                    {                        
                        CharBuffer[index].Char.AsciiChar = (byte)' ';
                    }
                    else if(y > ceiling && y <= floor)
                    {
                        CharBuffer[index].Attributes = 12;
                        CharBuffer[index].Char.AsciiChar = (byte)'#';
                    }
                    else
                    {
                        CharBuffer[index].Char.AsciiChar = (byte)' ';
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
    }    
}
