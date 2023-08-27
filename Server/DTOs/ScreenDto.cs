using System;

namespace ScreenTemperature.DTOs
{
    public class ScreenDto
    {
        public string DevicePath { get; set; }
        public string Label { get; set; }
        public bool IsPrimary { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
