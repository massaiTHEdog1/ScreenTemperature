using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace ScreenTemperature.Entities;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct PHYSICAL_MONITOR
{
    public IntPtr hPhysicalMonitor;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string szPhysicalMonitorDescription;
}


/// <summary>
/// Represents a screen attached to this machine.
/// </summary>
[NotMapped]
public class Screen
{
    /// <summary>
    /// Returns the identifier.
    /// </summary>
    public string DevicePath { get; set; }

    /// <summary>
    /// Returns the friendly name.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Returns whether this screen Is the primary screen.
    /// </summary>
    public bool IsPrimary { get; set; }
    
    /// <summary>
    /// Returns the X position.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Returns the Y position.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Returns the width.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Returns the height.
    /// </summary>
    public int Height { get; set; }

    public bool IsDDCSupported { get; set; }

    public bool IsBrightnessSupported { get; set; }

    public uint MaxBrightness { get; set; }
    public uint MinBrightness { get; set; }
    public uint CurrentBrightness { get; set; }

    public PHYSICAL_MONITOR[] PhysicalMonitors { get; set; }
}