using System;
using System.Numerics;

namespace GamepadApi;

/// <summary>
/// Represents a single physical gamepad (XInput slot 0..3).
/// </summary>
public sealed class GamepadDevice
{
    /// <summary>XInput user index (0..3).</summary>
    public int Index { get; }

    /// <summary>Deadzone configuration (modifiable at runtime).</summary>
    public DeadzoneConfig Deadzone { get; } = new();

    /// <summary>True if the device is currently connected.</summary>
    public bool IsConnected => TryGetState(out _);

    /// <summary>Last cached state (if any).</summary>
    public GamepadState? LastState { get; set; }

    public GamepadDevice(int index)
    {
        if (index is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(index));
        Index = index;
    }

    /// <summary>
    /// Attempts to read the current state from XInput. Returns false if not connected.
    /// </summary>
    public bool TryGetState(out GamepadState state)
    {
        var result = XInputNative.XInputGetState(Index, out var native);
        if (result != XInputNative.ERROR_SUCCESS)
        {
            state = default!;
            return false;
        }

        // Apply deadzones & normalize
        var ls = MathUtil.ApplyRadialDeadzone(native.Gamepad.sThumbLX, native.Gamepad.sThumbLY,
            Deadzone.LeftStickDeadzone, Deadzone.ClampAfterDeadzone);

        var rs = MathUtil.ApplyRadialDeadzone(native.Gamepad.sThumbRX, native.Gamepad.sThumbRY,
            Deadzone.RightStickDeadzone, Deadzone.ClampAfterDeadzone);

        var lt = native.Gamepad.bLeftTrigger <= Deadzone.TriggerThreshold ? 0f : MathUtil.NormalizeTrigger(native.Gamepad.bLeftTrigger);
        var rt = native.Gamepad.bRightTrigger <= Deadzone.TriggerThreshold ? 0f : MathUtil.NormalizeTrigger(native.Gamepad.bRightTrigger);

        state = new GamepadState
        {
            Buttons = (GamepadButtons)native.Gamepad.wButtons,
            LeftStick = new Vector2(ls.X, ls.Y),
            RightStick = new Vector2(rs.X, rs.Y),
            LeftTrigger = lt,
            RightTrigger = rt,
            PacketNumber = native.dwPacketNumber
        };
        return true;
    }

    /// <summary>
    /// Sets vibration speeds in [0..1]. Left = low-frequency, Right = high-frequency.
    /// </summary>
    public void SetVibration(float left, float right)
    {
        ushort l = (ushort)Math.Clamp((int)(left * 65535f), 0, 65535);
        ushort r = (ushort)Math.Clamp((int)(right * 65535f), 0, 65535);
        var vib = new XInputNative.XINPUT_VIBRATION { wLeftMotorSpeed = l, wRightMotorSpeed = r };
        _ = XInputNative.XInputSetState(Index, ref vib);
    }

    /// <summary>
    /// Stops vibration on this device.
    /// </summary>
    public void StopVibration() => SetVibration(0f, 0f);
}
