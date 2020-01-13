using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ConsoleStein.Input;
using ConsoleStein.Time;
using ConsoleStein.Maths;
using System.Collections.Generic;
using System.Linq;
using ConsoleStein.Components;

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

        private InputSystem inputSystem;

        private List<CameraComponent> Components { get; set; }

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

            Components = new List<CameraComponent>();
            var camera = CreateCamera();
            camera.ViewPort = new Rect(0f, 0f, 0.5f, 0.5f);
            camera.Entity.GetComponent<TransformComponent>().position = new Vector2(8f, 5f);

            var camera2 = CreateCamera();
            camera2.ViewPort = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            camera2.Entity.GetComponent<TransformComponent>().position = new Vector2(8f, 5f);
            Components.Add(camera);
            Components.Add(camera2);

            UpdateCharBuffer();
        }

        private CameraComponent CreateCamera()
        {
            var entity = new Entity();
            var transform = new TransformComponent();
            entity.AddComponent(typeof(TransformComponent), transform);
            var camera = new CameraComponent();
            entity.AddComponent(typeof(CameraComponent), camera);
            return camera;
        }

        public void Update()
        {
            if (Handle.IsInvalid)
                return;

            var transform = Components[0].Entity.GetComponent<TransformComponent>();
            if (inputSystem.GetKey(EKeyCode.A))
            {
                transform.rotation -= 0.5f * TimeSystem.DeltaTime;
            }
            if(inputSystem.GetKey(EKeyCode.D))
            {
                transform.rotation += 0.5f * TimeSystem.DeltaTime;
            }

            if(inputSystem.GetKey(EKeyCode.W))
            {
                transform.Translate(transform.forward, TimeSystem.DeltaTime);                

                if(Map[(int)transform.position.y * 16 + (int)transform.position.x] != '.')
                {
                    transform.Translate(transform.back, TimeSystem.DeltaTime);                    
                }
            }

            if (inputSystem.GetKey(EKeyCode.S))
            {
                transform.Translate(transform.back, TimeSystem.DeltaTime);                

                if (Map[(int)transform.position.y * 16 + (int)transform.position.x] != '.')
                {
                    transform.Translate(transform.forward, TimeSystem.DeltaTime);                    
                }
            }

            transform = Components[1].Entity.GetComponent<TransformComponent>();
            if (inputSystem.GetKey(EKeyCode.Left))
            {
                transform.rotation -= 0.5f * TimeSystem.DeltaTime;
            }
            if (inputSystem.GetKey(EKeyCode.Right))
            {
                transform.rotation += 0.5f * TimeSystem.DeltaTime;
            }

            if (inputSystem.GetKey(EKeyCode.Up))
            {
                transform.Translate(transform.forward, TimeSystem.DeltaTime);

                if (Map[(int)transform.position.y * 16 + (int)transform.position.x] != '.')
                {
                    transform.Translate(transform.back, TimeSystem.DeltaTime);
                }
            }

            if (inputSystem.GetKey(EKeyCode.Down))
            {
                transform.Translate(transform.back, TimeSystem.DeltaTime);

                if (Map[(int)transform.position.y * 16 + (int)transform.position.x] != '.')
                {
                    transform.Translate(transform.forward, TimeSystem.DeltaTime);
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
            for(int i = 0; i < Components.Count; i++)
            {
                var camera = Components[i];
                var cameraTransform = camera.Entity.GetComponent<TransformComponent>();
                short viewWidth = (short)(camera.ViewPort.width * ScreenWidth);
                short viewHeight = (short)(camera.ViewPort.height * ScreenHeight);
                short viewX = (short)(camera.ViewPort.x * ScreenWidth);
                short viewY = (short)(camera.ViewPort.y * ScreenHeight);
                for (short x = viewX; x < viewX + viewWidth; x++)
                {
                    float angle = cameraTransform.rotation - camera.FieldOfView / 2f + (x - viewX) / (float)ScreenWidth * camera.FieldOfView;
                    float dist = 0f;
                    bool hitWall = false;
                    bool hitBoundary = false;
                    short wallColor = 0;
                    float farClip = Math.Min(camera.FarClippingDistance, 16);

                    Vector2 eyeVec = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
                    while (!hitWall && dist < farClip)
                    {
                        dist += 0.1f;

                        int testX = (int)(cameraTransform.position.x + eyeVec.x * dist);
                        int testY = (int)(cameraTransform.position.y + eyeVec.y * dist);

                        if (testX < 0 || testX >= 16 || testY < 0 || testY >= 11)
                        {
                            hitWall = true;
                            dist = farClip;
                        }
                        else if (Map[testY * 16 + testX] != '.')
                        {
                            hitWall = true;

                            List<Vector2> p = new List<Vector2>();
                            for (int j = 0; j < 2; j++)
                            {
                                for (int k = 0; k < 2; k++)
                                {
                                    Vector2 vec = new Vector2
                                        (
                                            (float)testX + j - cameraTransform.position.x,
                                            (float)testY + k - cameraTransform.position.y
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

                    int ceiling = (int)((viewHeight / 2f) - viewHeight / dist);
                    int floor = viewHeight - ceiling;

                    byte wallShade = GetWallShade(dist, farClip);

                    if (hitBoundary)
                        wallShade = (byte)' ';
                    for (int y = viewY; y < viewY + viewHeight; y++)
                    {
                        byte floorShade = GetFloorShade(y - viewY, viewHeight);
                        int index = y * ScreenWidth + x;
                        if (y <= ceiling + viewY)
                        {
                            CharBuffer[index].Attributes = 1;
                            CharBuffer[index].Char.AsciiChar = (byte)'*';
                        }
                        else if (y > ceiling + viewY && y <= floor + viewY)
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

                PrintText(new RectInt(viewX, viewY, 16, 11), DisplayMap, ConsoleColor.Cyan);
                PrintText(new RectInt(viewX + (int)cameraTransform.position.x, viewY + 10 - (int)cameraTransform.position.y, 1, 1), ((char)GetRotationChar(cameraTransform.rotation)).ToString(), ConsoleColor.Yellow);
            }                        

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

        private byte GetWallShade(float dist, float clippingDistance)
        {
            if(dist <= clippingDistance / 4f)
            {
                return 219; 
            }
            if(dist < clippingDistance / 3f)
            {
                return 178;
            }
            if(dist < clippingDistance / 2f)
            {
                return 177;
            }
            if(dist < clippingDistance)
            {
                return 176;
            }
            return (byte)' ';
            
        }

        private byte GetFloorShade(int y, short viewHeight)
        {
            float b = 1f - ((y - viewHeight / 2f) / (viewHeight / 2f));
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
