
using ScreenTemperature.Entities;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using WindowsDisplayAPI.Native.Structures;

namespace ScreenTemperature;

internal static class Win32
{
    public const uint WS_OVERLAPPED = 0x00000000;
    public const uint WS_MAXIMIZEBOX = 0x00010000;
    public const uint WS_MINIMIZEBOX = 0x00020000;
    public const uint WS_THICKFRAME = 0x00040000;
    public const uint WS_SYSMENU = 0x00080000;
    public const uint WS_CAPTION = 0x00C00000;
    public const uint WS_VISIBLE = 0x10000000;
    public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

    public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WndProc lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [DllImport("user32.dll")]
    public static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll")]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreateWindowEx(
       uint dwExStyle,
       string lpClassName,
       string lpWindowName,
       uint dwStyle,
       int x,
       int y,
       int nWidth,
       int nHeight,
       IntPtr hWndParent,
       IntPtr hMenu,
       IntPtr hInstance,
       IntPtr lpParam
    );

    [DllImport("user32.dll", SetLastError = true)]
    public static extern short RegisterClassEx(ref WNDCLASSEX lpwcx);

    [DllImport("gdi32.dll")]
    public static extern bool SetDeviceGammaRamp(int hdc, IntPtr ramp);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateDC(string lpszDriver, string? lpszDevice, string? lpszOutput, IntPtr lpInitData);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("Dxva2.dll")]
    public static extern bool GetMonitorBrightness(IntPtr hdc, ref uint pdwMinimumBrightness, ref uint pdwCurrentBrightness, ref uint pdwMaximumBrightness);

    [DllImport("Dxva2.dll")]
    public static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

    [DllImport("Dxva2.dll")]
    public static extern bool GetMonitorCapabilities(IntPtr hdc, out MC_CAPS pdwMonitorCapabilities, out MC_SUPPORTED_COLOR_TEMPERATURE pdwSupportedColorTemperatures);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

    public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    [Flags]
    internal enum MonitorInfoFlags : uint
    {
        None = 0,
        Primary = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfo
    {
        internal uint Size;
        public readonly Rect Bounds;
        public readonly Rect WorkingArea;
        public readonly MonitorInfoFlags Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public readonly string DisplayName;
    }

    [DllImport("user32")]
    public static extern bool GetMonitorInfo(IntPtr monitorHandle, ref MonitorInfo monitorInfo);

    [DllImport("Dxva2.dll")]
    public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

    [DllImport("Dxva2.dll")]
    public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    [DllImport("Dxva2.dll")]
    public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

    // found on https://github.com/emoacht/Monitorian/blob/master/Source/Monitorian.Core/Models/Monitor/MonitorConfiguration.cs
    [Flags]
    public enum MC_CAPS
    {
        MC_CAPS_NONE = 0x00000000,
        MC_CAPS_MONITOR_TECHNOLOGY_TYPE = 0x00000001,
        MC_CAPS_BRIGHTNESS = 0x00000002,
        MC_CAPS_CONTRAST = 0x00000004,
        MC_CAPS_COLOR_TEMPERATURE = 0x00000008,
        MC_CAPS_RED_GREEN_BLUE_GAIN = 0x00000010,
        MC_CAPS_RED_GREEN_BLUE_DRIVE = 0x00000020,
        MC_CAPS_DEGAUSS = 0x00000040,
        MC_CAPS_DISPLAY_AREA_POSITION = 0x00000080,
        MC_CAPS_DISPLAY_AREA_SIZE = 0x00000100,
        MC_CAPS_RESTORE_FACTORY_DEFAULTS = 0x00000400,
        MC_CAPS_RESTORE_FACTORY_COLOR_DEFAULTS = 0x00000800,
        MC_RESTORE_FACTORY_DEFAULTS_ENABLES_MONITOR_SETTINGS = 0x00001000
    }

    // found on https://github.com/emoacht/Monitorian/blob/master/Source/Monitorian.Core/Models/Monitor/MonitorConfiguration.cs
    [Flags]
    public enum MC_SUPPORTED_COLOR_TEMPERATURE
    {
        MC_SUPPORTED_COLOR_TEMPERATURE_NONE = 0x00000000,
        MC_SUPPORTED_COLOR_TEMPERATURE_4000K = 0x00000001,
        MC_SUPPORTED_COLOR_TEMPERATURE_5000K = 0x00000002,
        MC_SUPPORTED_COLOR_TEMPERATURE_6500K = 0x00000004,
        MC_SUPPORTED_COLOR_TEMPERATURE_7500K = 0x00000008,
        MC_SUPPORTED_COLOR_TEMPERATURE_8200K = 0x00000010,
        MC_SUPPORTED_COLOR_TEMPERATURE_9300K = 0x00000020,
        MC_SUPPORTED_COLOR_TEMPERATURE_10000K = 0x00000040,
        MC_SUPPORTED_COLOR_TEMPERATURE_11500K = 0x00000080
    }

    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public UIntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [Flags]
    public enum KeyModifiers : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4
    }

    [DllImport("user32.dll")]
    public static extern uint GetQueueStatus(uint flags);

    [DllImport("user32.dll")]
    public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("Kernel32.dll")]
    public static extern uint GetLastError();

    [DllImport("user32.dll")]
    public static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
           uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage(ref MSG lpmsg);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [StructLayout(LayoutKind.Sequential)]
    public struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hwnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
    }

    public const int NIF_MESSAGE = 0x00000001;
    public const int NIF_ICON = 0x00000002;
    public const int NIF_TIP = 0x00000004;
    public const int NIF_SHOWTIP = 0x00000080;
    public const int NIF_INFO = 0x00000010;
    public const int NOTIFYICON_VERSION_4 = 4;
    public const int NIM_SETVERSION = 0x00000004;

    public const int WM_USER = 0x0400;
    public const int WM_TRAYICON = WM_USER + 1;
    public const int IDI_APPLICATION = 32512;

    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_RBUTTONDOWN = 0x0204;
    public const int WM_DESTROY = 0x0002;
    public const int WM_HOTKEY = 0x0312;
    public const int WM_SIZE = 0x0005;
    public const int WM_QUIT = 0x0012;

    [DllImport("Shell32.dll")]
    public static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("Comctl32.dll")]
    public static extern long LoadIconMetric(IntPtr hinst, string pszName, int lims, ref IntPtr phico);

    [DllImport("user32.dll")]
    public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    public const uint IMAGE_ICON = 1;
    public const uint LR_LOADFROMFILE = 0x00000010;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr LoadImage(IntPtr hInstance, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    public const int SW_HIDE = 0;
    public const int SW_SHOWNORMAL = 1;
    public const int SW_SHOWMINIMIZED = 2;
    public const int SW_SHOWMAXIMIZED = 3;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SW_SHOW = 5;
    public const int SW_MINIMIZE = 6;
    public const int SW_SHOWMINNOACTIVE = 7;
    public const int SW_SHOWNA = 8;
    public const int SW_RESTORE = 9;
    public const int SW_SHOWDEFAULT = 10;
    public const int SW_FORCEMINIMIZE = 11;

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool UpdateWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AllocConsole();
}