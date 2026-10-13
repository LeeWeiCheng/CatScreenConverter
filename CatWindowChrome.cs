using System.Runtime.InteropServices;

namespace CatVideo;

internal static class CatWindowChrome
{
    public static void Apply(IntPtr window)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)) return;
        Set(window, 33, 2); // DWMWA_WINDOW_CORNER_PREFERENCE: rounded corners.
        Set(window, 34, 237 | (216 << 8) | (183 << 16));
        Set(window, 35, 245 | (224 << 8) | (195 << 16));
        Set(window, 36, 91 | (43 << 8) | (25 << 16));
    }
    private static void Set(IntPtr window, int attribute, int value)
    {
        // Optional window styling; an unsupported attribute should not prevent
        // opening the app or registering the restoration shortcut.
        _ = DwmSetWindowAttribute(window, attribute, ref value, sizeof(int));
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

}
