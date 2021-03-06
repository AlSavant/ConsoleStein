﻿using System.Diagnostics;

namespace ConsoleStein.Time
{
    public sealed class TimeSystem
    {
        public static float DeltaTime { get; set; } = 0f;
        public static float Time { get; set; } = 0f;

        private Stopwatch stopWatch;

        public void Setup()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        public void Update()
        {
            float total = (float)stopWatch.Elapsed.TotalSeconds;
            DeltaTime = total - Time;
            Time = total;
        }
    }
}
