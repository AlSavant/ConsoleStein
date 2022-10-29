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
using ConsoleStein.Util;
using ConsoleStein.Assets;

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
        private float[] DepthBuffer { get; set; }
        private SmallRect bufferRect;
        private string Map { get; set; }
        private string TerrainMap { get; set; }
        private string DisplayMap { get; set; }

        public Vector2Int ScreenSize { get; set; }        

        private InputSystem inputSystem;

        private List<ICameraComponent> Cameras { get; set; }
        private List<IRendererComponent> Renderers { get; set; }
        private ConsoleMaterial wallMaterial { get; set; }
        private SkyboxMaterial skybox { get; set; }
        private ConsoleSprite floorSprite { get; set; }

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

            TerrainMap = string.Empty;
            TerrainMap += "MMMMMMMMMMMMMMMM";
            TerrainMap += "M,,,,,,,,,,,,,,M";
            TerrainMap += "M,,,,,........,M";
            TerrainMap += "MFFFF,........,M";
            TerrainMap += "M,,,,,........,M";
            TerrainMap += "M,........,,,,,M";
            TerrainMap += "M,........,OOOOM";
            TerrainMap += "M,........,,,,,M";
            TerrainMap += "M,............,M";
            TerrainMap += "M,,,,,,,,,,,,,,M";
            TerrainMap += "MMMMMMMMMMMMMMMM";

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

            Cameras = new List<ICameraComponent>();
            var camera = CreateCamera();
            camera.ViewPort = new Rect(0f, 0f, 1f, 1f);
            camera.Entity.GetComponent<ITransformComponent>().Position = new Vector2(8f, 5f);
            Cameras.Add(camera);

            //var camera2 = CreateCamera();
            //camera2.ViewPort = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            //camera2.Entity.GetComponent<ITransformComponent>().Position = new Vector2(8f, 5f);
            //Cameras.Add(camera2);

            wallMaterial = resourcesSystem.Load<ConsoleMaterial>("Materials/BrickMaterial");
            floorSprite = resourcesSystem.Load<ConsoleSprite>("Textures/brick_wall");

            Renderers = new List<IRendererComponent>();
            var cat = CreateCat();
            cat.Entity.GetComponent<ITransformComponent>().Position = new Vector2(8.5f, 8.5f);
            cat.Entity.GetComponent<IRendererComponent>().Material = resourcesSystem.Load<ConsoleMaterial>("Materials/CatMaterial");
            Renderers.Add(cat);

            cat = CreateCat();
            cat.Entity.GetComponent<ITransformComponent>().Position = new Vector2(9.5f, 9.5f);
            cat.Entity.GetComponent<IRendererComponent>().Material = resourcesSystem.Load<ConsoleMaterial>("Materials/CatMaterial");
            Renderers.Add(cat);

            UpdateCharBuffer();

            skybox = resourcesSystem.Load<SkyboxMaterial>("Materials/Skybox");
        }

        private RendererComponent CreateCat()
        {
            var entity = new Entity();
            var transform = new TransformComponent();
            entity.AddComponent(typeof(ITransformComponent), transform);
            var renderer = new RendererComponent();
            entity.AddComponent(typeof(IRendererComponent), renderer);
            return renderer;
        }

        private ICameraComponent CreateCamera()
        {
            var entity = new Entity();
            var transform = new TransformComponent();
            entity.AddComponent(typeof(ITransformComponent), transform);
            var camera = new CameraComponent();
            entity.AddComponent(typeof(ICameraComponent), camera);
            return camera;
        }

        public void Update()
        {
            if (Handle.IsInvalid)
                return;
            skybox.layers[0].rotation += TimeSystem.DeltaTime * 0.01f;
            //skybox.layers[1].rotation += TimeSystem.DeltaTime * 0.005f;
            var transform = Cameras[0].Entity.GetComponent<ITransformComponent>();
            if (inputSystem.GetKey(EKeyCode.A))
            {
                transform.Rotation -= 0.5f * TimeSystem.DeltaTime;
            }
            if(inputSystem.GetKey(EKeyCode.D))
            {
                transform.Rotation += 0.5f * TimeSystem.DeltaTime;
            }

            if(inputSystem.GetKey(EKeyCode.W))
            {
                transform.Translate(transform.Forward, TimeSystem.DeltaTime);                

                if(Map[(int)transform.Position.y * 16 + (int)transform.Position.x] != '.')
                {
                    transform.Translate(transform.Back, TimeSystem.DeltaTime);                    
                }
            }

            if (inputSystem.GetKey(EKeyCode.S))
            {
                transform.Translate(transform.Back, TimeSystem.DeltaTime);                

                if (Map[(int)transform.Position.y * 16 + (int)transform.Position.x] != '.')
                {
                    transform.Translate(transform.Forward, TimeSystem.DeltaTime);                    
                }
            }

            /*transform = Components[1].Entity.GetComponent<ITransformComponent>();
            if (inputSystem.GetKey(EKeyCode.Left))
            {
                transform.Rotation -= 0.5f * TimeSystem.DeltaTime;
            }
            if (inputSystem.GetKey(EKeyCode.Right))
            {
                transform.Rotation += 0.5f * TimeSystem.DeltaTime;
            }

            if (inputSystem.GetKey(EKeyCode.Up))
            {
                transform.Translate(transform.Forward, TimeSystem.DeltaTime);

                if (Map[(int)transform.Position.y * 16 + (int)transform.Position.x] != '.')
                {
                    transform.Translate(transform.Back, TimeSystem.DeltaTime);
                }
            }

            if (inputSystem.GetKey(EKeyCode.Down))
            {
                transform.Translate(transform.Back, TimeSystem.DeltaTime);

                if (Map[(int)transform.Position.y * 16 + (int)transform.Position.x] != '.')
                {
                    transform.Translate(transform.Forward, TimeSystem.DeltaTime);
                }
            }*/
            var tempSize = new Vector2Int(Console.WindowWidth, Console.WindowHeight);
            if(tempSize != ScreenSize)
            {
                ScreenSize = tempSize;                
                UpdateCharBuffer();
            }             
            for(int i = 0; i < Cameras.Count; i++)
            {
                var camera = Cameras[i];
                var cameraTransform = camera.Entity.GetComponent<ITransformComponent>();
                RectInt view = new RectInt
                    (
                        (int)(camera.ViewPort.x * ScreenSize.x),
                        (int)(camera.ViewPort.y * ScreenSize.y),
                        (int)(camera.ViewPort.width * ScreenSize.x),
                        (int)(camera.ViewPort.height * ScreenSize.y)
                    );
                for (int x = view.x; x < view.x + view.width; x++)
                {
                    float angle = cameraTransform.Rotation - camera.FieldOfView / 2f + (x - view.x) / (float)ScreenSize.x * camera.FieldOfView;
                    float dist = 0f;
                    bool hitWall = false;

                    float farClip = Math.Min(camera.FarClippingDistance, 16);

                    Vector2 eyeVec = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
                    Vector2 uv = Vector2.zero;
                    while (!hitWall && dist < farClip)
                    {
                        dist += 0.01f;

                        Vector2Int test = new Vector2Int
                            (
                                (int)(cameraTransform.Position.x + eyeVec.x * dist),
                                (int)(cameraTransform.Position.y + eyeVec.y * dist)
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
                                transform.Position.x + eyeVec.x * dist,
                                transform.Position.y + eyeVec.y * dist
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
                    DepthBuffer[x] = dist;
                    for (int y = view.y; y < view.y + view.height; y++)
                    {                        
                        int index = y * ScreenSize.x + x;
                        if (ceiling != 0 && y <= ceiling + view.y)
                        {
                            byte[] pix = null;
                            foreach(var layer in skybox.layers)
                            {
                                float degrees = (angle + layer.rotation) * (180f / (float)Math.PI);
                                degrees %= 360;
                                if (degrees < 0)
                                    degrees += 360;
                                float skyX = degrees / 360f;
                                float skyY = (y - view.y) / (view.height / 2f);
                                var skyColor = layer.texture.SamplePixel(new Vector2(skyX, skyY));
                                if (skyColor == null || skyColor[0] == ' ' || skyColor[1] == 0)
                                    continue;
                                pix = skyColor;
                                break;
                            }
                            if(pix != null)
                            {
                                CharBuffer[index].Attributes = pix[1];
                                CharBuffer[index].Char.AsciiChar = pix[0];
                            }
                            else
                            {
                                CharBuffer[index].Attributes = 0;
                                CharBuffer[index].Char.AsciiChar = 0;
                            }
                            
                        }
                        else if (y > ceiling + view.y && y <= floor + view.y)
                        {
                            uv.y = (y - (float)(ceiling + view.y)) / ((floor + view.y + 1) - (float)(ceiling + view.y));
                            
                            if(wallMaterial != null)
                            {

                                var pixel = wallMaterial.SamplePixel(uv);
                                if(pixel != null)
                                {
                                    CharBuffer[index].Attributes = pixel[1];
                                    CharBuffer[index].Char.AsciiChar = pixel[0];
                                }
                            }
                        }
                        else
                        {
                            byte[] floorShade = GetFloorShade(y - view.y, view.height, transform.Position, eyeVec, dist, view.height - floor);
                            CharBuffer[index].Attributes = floorShade[1];
                            CharBuffer[index].Char.AsciiChar = floorShade[0];
                        }
                    }
                }
                Dictionary<Vector2Int, float> depthBuffer = new Dictionary<Vector2Int, float>();                
                foreach(var renderer in Renderers)
                {
                    var rendererTransform = renderer.Entity.GetComponent<ITransformComponent>();
                    var dist = Vector2.Distance(transform.Position, rendererTransform.Position);
                    var dir = rendererTransform.Position - transform.Position;
                    Vector2 eye = new Vector2((float)Math.Sin(transform.Rotation), (float)Math.Cos(transform.Rotation));

                    float angleDiff = (float)(Math.Atan2(eye.y, eye.x) - Math.Atan2(dir.y, dir.x));
                    if (angleDiff < -Math.PI)
                        angleDiff += 2f * (float)Math.PI;
                    if (angleDiff > Math.PI)
                        angleDiff -= 2f * (float)Math.PI;

                    if (Math.Abs(angleDiff) >= camera.FieldOfView)
                        continue;
                    if (dist < camera.NearClippingDistance)
                        continue;
                    if (dist > camera.FarClippingDistance)
                        continue;
                    float objectCeiling = (int)((view.height / 2f) - view.height / dist);
                    float objectFloor = view.height - objectCeiling;
                    float objectHeight = objectFloor - objectCeiling;
                    float objectAspectRatio = renderer.Material.texture.Height / (float)renderer.Material.texture.Width;
                    float objectWidth = objectHeight / objectAspectRatio;
                    float middle = view.x + (0.5f * (angleDiff / (camera.FieldOfView / 2f)) + 0.5f) * view.width;

                    for(float lx = 0; lx < objectWidth; lx++)
                    {
                        for(float ly = 0; ly < objectHeight; ly++)
                        {
                            Vector2 uv = new Vector2(lx / objectWidth, ly / objectHeight);
                            var pixel = renderer.Material.texture.SamplePixel(uv);
                            if (pixel == null)
                                continue;
                            Vector2Int objectPos = new Vector2Int
                                (
                                    (int)(middle + lx - (objectWidth / 2f)),
                                    (int)(objectCeiling + ly)
                                );
                            if (objectPos.x < view.x)
                                continue;
                            if (objectPos.x >= view.x + view.width)
                                continue;
                            if (objectPos.y < 0)
                                continue;
                            if (objectPos.y >= view.y + view.height)
                                continue;
                            if (renderer.Material.texture.IsTransparent && pixel[0] == 0 || pixel[1] == 0)
                                continue;
                            if (DepthBuffer[objectPos.x] < dist)
                                continue;
                            if (!depthBuffer.ContainsKey(objectPos)) 
                            {
                                depthBuffer.Add(objectPos, dist);
                            }
                            if (dist > depthBuffer[objectPos])
                                continue;

                            int index = objectPos.y * ScreenSize.x + objectPos.x;
                            CharBuffer[index].Attributes = pixel[1];
                            CharBuffer[index].Char.AsciiChar = pixel[0];
                            depthBuffer[objectPos] = dist;
                        }
                    }
                }

                PrintText(new RectInt(view.x, view.y, 16, 11), DisplayMap, ConsoleColor.Cyan);
                PrintText(new RectInt(view.x + (int)cameraTransform.Position.x, view.y + 10 - (int)cameraTransform.Position.y, 1, 1), ((char)GetRotationChar(cameraTransform.Rotation)).ToString(), ConsoleColor.Yellow);
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
            DepthBuffer = new float[ScreenSize.x];
            bufferRect = new SmallRect() { Left = 0, Top = 0, Right = (short)ScreenSize.x, Bottom = (short)ScreenSize.y };
        }        

        private byte[] GetFloorShade(int y, int viewHeight, Vector2 position, Vector2 direction, float hitDistance, int floorLines)
        {
            var b = Math.Abs((y - viewHeight) / (float)floorLines);            
            hitDistance *= b;
            Vector2 terrainLocation = position + direction * hitDistance;            
            byte color = 0;
            Vector2Int test = new Vector2Int((int)terrainLocation.x, (int)terrainLocation.y);
            if (test.x < 0 || test.x >= 16 || test.y < 0 || test.y >= 11)
            {
                color = 0;
            }
            else if (TerrainMap[test.y * 16 + test.x] == '.')
            {
                color = 2;
            }                
            else if (TerrainMap[test.y * 16 + test.x] == ',')
            {
                color = 1;
            }                      
            
            if (b < 0.25f)
            {
                return new byte[] { (byte)'#', color };
            }
            else if (b < 0.5f)
            {
                return new byte[] { (byte)'x', color };                
            }
            else if (b < 0.75f)
            {
                return new byte[] { (byte)'-', color };                
            }
            else if (b < 1f)
            {
                return new byte[] { (byte)'.', color };
            }
            return new byte[] { (byte)' ', 0 };

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
