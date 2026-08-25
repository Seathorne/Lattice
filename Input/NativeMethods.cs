using System.Runtime.InteropServices;

namespace Lattice.Input;

internal static class NativeMethods
{
    [DllImport("user32.dll")]
    internal static extern short GetAsyncKeyState(int virtualKey);

    internal static bool IsHeld(int virtualKey)
        => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
}