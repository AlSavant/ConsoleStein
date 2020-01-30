using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ConsoleStein.Input;
using ConsoleStein.Time;
using ConsoleStein.Maths;
using ConsoleStein.Util;
using System.Collections.Generic;
using ConsoleStein.Components;
using System.Runtime.Serialization.Formatters.Binary;

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

        private ConsoleSprite wallSprite { get; set; }

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
            camera.ViewPort = new Rect(0f, 0f, 1f, 1f);
            camera.Entity.GetComponent<TransformComponent>().position = new Vector2(8f, 5f);

            var camera2 = CreateCamera();
            camera2.ViewPort = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            camera2.Entity.GetComponent<TransformComponent>().position = new Vector2(8f, 5f);
            Components.Add(camera);
            //Components.Add(camera2);

            var path = AppDomain.CurrentDomain.BaseDirectory;
            path += "/Resources/Textures/brick_wall.csp";
            if(File.Exists(path))
            {
                var formatter = new BinaryFormatter();
                formatter.Binder = new ConsoleSpriteConverter();
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                wallSprite = (ConsoleSprite)formatter.Deserialize(stream);
                stream.Close();
            }            
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

            /*transform = Components[1].Entity.GetComponent<TransformComponent>();
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
            }*/

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

                    float farClip = Math.Min(camera.FarClippingDistance, 16);

                    Vector2 eyeVec = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
                    Vector2 uv = Vector2.zero;
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

                            Vector2 blockMid = new Vector2(testX + 0.5f, testY + 0.5f);
                            Vector2 testPoint = new Vector2
                                (
                                transform.position.x + eyeVec.x * dist,
                                transform.position.y + eyeVec.y * dist
                                );

                            float testAngle = (float)Math.Atan2(testPoint.y - blockMid.y, testPoint.x - blockMid.x);
                            
                            if(testAngle >= -Math.PI * 0.25f && testAngle < Math.PI * 0.25f)
                            {
                                uv.x = testPoint.y - testY;
                            }
                            if(testAngle >= Math.PI * 0.25f && testAngle < Math.PI * 0.75f)
                            {
                                uv.x = testPoint.x - testX;
                            }
                            if(testAngle < -Math.PI * 0.25f && testAngle >= -Math.PI * 0.75f)
                            {
                                uv.x = testPoint.x - testX;
                            }
                            if(testAngle >= Math.PI * 0.75f || testAngle < -Math.PI * 0.75f)
                            {
                                uv.x = testPoint.y - testY;
                            }
                        }
                    }

                    int ceiling = (int)((viewHeight / 2f) - viewHeight / dist);
                    int floor = viewHeight - ceiling;
                    
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
                            uv.y = (y - (float)ceiling) / (floor + 1 - (float)ceiling);
                            
                            if(wallSprite != null)
                            {
                                var pixel = wallSprite.SamplePixel(uv);
                                if(pixel != null)
                                {
                                    CharBuffer[index].Attributes = pixel[1];
                                    CharBuffer[index].Char.AsciiChar = pixel[0];
                                }
                            }
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
    }    
}
