using System;
using System.Runtime.InteropServices;

namespace GamepadApi;

/// <summary>
/// Minimal P/Invoke layer for XInput (Windows).
/// </summary>
internal static class XInputNative
{
    private const string XINPUT_DLL = "xinput1_4.dll"; // Win8+; fallback chain below.

    // If loading xinput1_4 fails on older systems, you could optionally try:
    // xinput1_3.dll or xinput9_1_0.dll via manual LoadLibrary/GetProcAddress,
    // but for simplicity we directly import xinput1_4 (works on modern Windows).

    [DllImport(XINPUT_DLL, EntryPoint = "XInputGetState")]
    internal static extern int XInputGetState(int dwUserIndex, out XINPUT_STATE pState);

    [DllImport(XINPUT_DLL, EntryPoint = "XInputSetState")]
    internal static extern int XInputSetState(int dwUserIndex, ref XINPUT_VIBRATION pVibration);

    internal const int ERROR_SUCCESS = 0;
    internal const int ERROR_DEVICE_NOT_CONNECTED = 1167;

    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_STATE
    {
        public uint dwPacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_GAMEPAD
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_VIBRATION
    {
        public ushort wLeftMotorSpeed;  // 0-65535
        public ushort wRightMotorSpeed; // 0-65535
    }
}
