using System;
using System.Numerics;

namespace GamepadApi;

/// <summary>
/// Logical buttons exposed by the API (XInput mapping).
/// </summary>
[Flags]
public enum GamepadButtons : ushort
{
    None = 0,
    DPadUp = 0x0001,
    DPadDown = 0x0002,
    DPadLeft = 0x0004,
    DPadRight = 0x0008,
    Start = 0x0010,
    Back = 0x0020,
    LeftThumb = 0x0040,
    RightThumb = 0x0080,
    LeftShoulder = 0x0100,
    RightShoulder = 0x0200,
    A = 0x1000,
    B = 0x2000,
    X = 0x4000,
    Y = 0x8000,
}

/// <summary>
/// Immutable snapshot of a gamepad's state, normalized and raw.
/// </summary>
public sealed class GamepadState
{
    /// <summary>Bitfield of pressed buttons.</summary>
    public GamepadButtons Buttons { get; init; }

    /// <summary>Left thumbstick normalized to [-1, 1].</summary>
    public Vector2 LeftStick { get; init; }

    /// <summary>Right thumbstick normalized to [-1, 1].</summary>
    public Vector2 RightStick { get; init; }

    /// <summary>Left trigger [0..1].</summary>
    public float LeftTrigger { get; init; }

    /// <summary>Right trigger [0..1].</summary>
    public float RightTrigger { get; init; }

    /// <summary>Raw packet number from XInput (increments on change).</summary>
    public uint PacketNumber { get; init; }

    public override string ToString()
        => $"Buttons={Buttons}, LS={LeftStick}, RS={RightStick}, LT={LeftTrigger:F2}, RT={RightTrigger:F2}, Pkt={PacketNumber}";
}

/// <summary>
/// Deadzone configuration for sticks and triggers.
/// </summary>
public sealed class DeadzoneConfig
{
    /// <summary>Radial deadzone for left stick (raw units). Default 7849.</summary>
    public int LeftStickDeadzone { get; set; } = 7849; // XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE

    /// <summary>Radial deadzone for right stick (raw units). Default 8689.</summary>
    public int RightStickDeadzone { get; set; } = 8689; // XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE

    /// <summary>Threshold for triggers (raw units). Default 30.</summary>
    public byte TriggerThreshold { get; set; } = 30;    // XINPUT_GAMEPAD_TRIGGER_THRESHOLD

    /// <summary>Clamp and normalize after deadzone (true) or rescale (false). Default: rescale.</summary>
    public bool ClampAfterDeadzone { get; set; } = false;
}

/// <summary>
/// Utility helpers for clamping and normalization.
/// </summary>
internal static class MathUtil
{
    public static float NormalizeStick(short value, short maxMagnitude) =>
        Math.Clamp(value / (value < 0 ? (float)short.MinValue : (float)short.MaxValue), -1f, 1f);

    public static float NormalizeTrigger(byte value) =>
        Math.Clamp(value / 255f, 0f, 1f);

    public static Vector2 ApplyRadialDeadzone(short x, short y, int deadzone, bool clampAfterDeadzone)
    {
        // Convert to float vector
        var v = new Vector2(x, y);
        var mag = v.Length();

        if (mag <= deadzone) return Vector2.Zero;

        var n = v / mag; // direction
        var max = 32767f;

        if (clampAfterDeadzone)
        {
            // simple clamp: subtract deadzone then scale to [-1..1]
            var scaled = (mag - deadzone) / (max - deadzone);
            scaled = Math.Clamp(scaled, 0f, 1f);
            return n * scaled;
        }
        else
        {
            // rescale so edge still reaches 1.0 smoothly
            var scaled = (mag - deadzone) / (max - deadzone);
            return n * Math.Clamp(scaled, 0f, 1f);
        }
    }
}
