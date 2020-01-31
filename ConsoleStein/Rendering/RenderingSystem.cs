using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ConsoleStein.Input;
using ConsoleStein.Time;
using ConsoleStein.Maths;
using ConsoleStein.Resources;
using System.Collections.Generic;
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

        public Vector2Int ScreenSize { get; set; }        

        private InputSystem inputSystem;

        private List<CameraComponent> Components { get; set; }

        private ConsoleSprite wallSprite { get; set; }

        public void Setup(InputSystem inputSystem, ResourcesSystem resourcesSystem)
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
            ScreenSize = new Vector2Int(Console.WindowWidth, Console.WindowHeight);

            Components = new List<CameraComponent>();
            var camera = CreateCamera();
            camera.ViewPort = new Rect(0f, 0f, 1f, 1f);
            camera.Entity.GetComponent<TransformComponent>().position = new Vector2(8f, 5f);

            var camera2 = CreateCamera();
            camera2.ViewPort = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            camera2.Entity.GetComponent<TransformComponent>().position = new Vector2(8f, 5f);
            Components.Add(camera);
            //Components.Add(camera2);

            wallSprite = resourcesSystem.Load<ConsoleSprite>("Textures/brick_wall");                      
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
            var tempSize = new Vector2Int(Console.WindowWidth, Console.WindowHeight);
            if(tempSize != ScreenSize)
            {
                ScreenSize = tempSize;                
                UpdateCharBuffer();
            }             
            for(int i = 0; i < Components.Count; i++)
            {
                var camera = Components[i];
                var cameraTransform = camera.Entity.GetComponent<TransformComponent>();
                RectInt view = new RectInt
                    (
                        (int)(camera.ViewPort.x * ScreenSize.x),
                        (int)(camera.ViewPort.y * ScreenSize.y),
                        (int)(camera.ViewPort.width * ScreenSize.x),
                        (int)(camera.ViewPort.height * ScreenSize.y)
                    );
                for (int x = view.x; x < view.x + view.width; x++)
                {
                    float angle = cameraTransform.rotation - camera.FieldOfView / 2f + (x - view.x) / (float)ScreenSize.x * camera.FieldOfView;
                    float dist = 0f;
                    bool hitWall = false;

                    float farClip = Math.Min(camera.FarClippingDistance, 16);

                    Vector2 eyeVec = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
                    Vector2 uv = Vector2.zero;
                    while (!hitWall && dist < farClip)
                    {
                        dist += 0.1f;

                        Vector2Int test = new Vector2Int
                            (
                                (int)(cameraTransform.position.x + eyeVec.x * dist),
                                (int)(cameraTransform.position.y + eyeVec.y * dist)
                            );                        

                        if (test.x < 0 || test.x >= 16 || test.y < 0 || test.y >= 11)
                        {
                            hitWall = true;
                            dist = farClip;
                        }
                        else if (Map[test.y * 16 + test.x] != '.')
                        {
                            hitWall = true;

                            Vector2 blockMid = new Vector2(test.x + 0.5f, test.y + 0.5f);
                            Vector2 testPoint = new Vector2
                                (
                                transform.position.x + eyeVec.x * dist,
                                transform.position.y + eyeVec.y * dist
                                );

                            float testAngle = (float)Math.Atan2(testPoint.y - blockMid.y, testPoint.x - blockMid.x);
                            
                            if(testAngle >= -Math.PI * 0.25f && testAngle < Math.PI * 0.25f)
                            {
                                uv.x = testPoint.y - test.y;
                            }
                            if(testAngle >= Math.PI * 0.25f && testAngle < Math.PI * 0.75f)
                            {
                                uv.x = testPoint.x - test.x;
                            }
                            if(testAngle < -Math.PI * 0.25f && testAngle >= -Math.PI * 0.75f)
                            {
                                uv.x = testPoint.x - test.x;
                            }
                            if(testAngle >= Math.PI * 0.75f || testAngle < -Math.PI * 0.75f)
                            {
                                uv.x = testPoint.y - test.y;
                            }
                        }
                    }

                    int ceiling = (int)((view.height / 2f) - view.height / dist);
                    int floor = view.height - ceiling;
                    
                    for (int y = view.y; y < view.y + view.height; y++)
                    {
                        byte floorShade = GetFloorShade(y - view.y, view.height);
                        int index = y * ScreenSize.x + x;
                        if (y <= ceiling + view.y)
                        {
                            CharBuffer[index].Attributes = 1;
                            CharBuffer[index].Char.AsciiChar = (byte)'*';
                        }
                        else if (y > ceiling + view.y && y <= floor + view.y)
                        {
                            uv.y = (y - (float)(ceiling + view.y)) / ((floor + view.y + 1) - (float)(ceiling + view.y));
                            
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

                PrintText(new RectInt(view.x, view.y, 16, 11), DisplayMap, ConsoleColor.Cyan);
                PrintText(new RectInt(view.x + (int)cameraTransform.position.x, view.y + 10 - (int)cameraTransform.position.y, 1, 1), ((char)GetRotationChar(cameraTransform.rotation)).ToString(), ConsoleColor.Yellow);
            }                        

            WriteConsoleOutput(Handle, CharBuffer,
                     new Coord() { X = (short)ScreenSize.x, Y = (short)ScreenSize.y },
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
                    int screenIndex = (rect.y + y) * ScreenSize.x + rect.x + x;
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
            CharBuffer = new CharInfo[ScreenSize.x * ScreenSize.y];
            bufferRect = new SmallRect() { Left = 0, Top = 0, Right = (short)ScreenSize.x, Bottom = (short)ScreenSize.y };
        }        

        private byte GetFloorShade(int y, int viewHeight)
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
