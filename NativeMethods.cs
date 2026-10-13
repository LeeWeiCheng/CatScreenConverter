using System.Runtime.InteropServices;

namespace CatVideo;

[StructLayout(LayoutKind.Sequential)]
internal struct ColorEffect
{
    // Native MAGCOLOREFFECT: 25 contiguous 32-bit floats, in row-major order.
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
    public float[] Values;
}

internal static class NativeMethods
{
    [DllImport("Magnification.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool MagInitialize();

    [DllImport("Magnification.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool MagUninitialize();

    [DllImport("Magnification.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool MagGetFullscreenColorEffect(out ColorEffect effect);

    [DllImport("Magnification.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool MagSetFullscreenColorEffect(ref ColorEffect effect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnregisterHotKey(IntPtr window, int id);
}
