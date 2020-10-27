using System;
using System.ComponentModel;
using System.Diagnostics;

namespace HiPerfTiming
{
    internal class HiPerformanceTimer
    {
        private long freq;
        private Stopwatch sw;

        public HiPerformanceTimer()
        {

            sw = new Stopwatch();
            freq = Stopwatch.Frequency;
        }

        // Start the timer
        public void Start()
        {
            sw.Reset();
            sw.Start();
        }

        // Stop the timer
        public void Stop()
        {
            sw.Stop();
        }

        // Returns the duration of the timer (by default in microseconds)
        public double Duration(bool nanosec = false, int precision = 2)
        {
            int multiplier = 1000000;

            if (nanosec)
                multiplier *= 1000;

            double timing = (double)(sw.ElapsedTicks) * multiplier / (double)freq;

            if (precision < 0)
                precision = 2;

            timing = (double)Decimal.Round((decimal)timing, precision);

            return timing;
        }

        // Returns ticks frequency ( number of ticks per second ) 
        public double Freq
        {
            get
            {
                return freq;
            }
        }
    }
}