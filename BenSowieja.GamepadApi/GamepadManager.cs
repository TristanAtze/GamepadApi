using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GamepadApi;

/// <summary>
/// Manages up to 4 XInput gamepads, provides polling and events.
/// </summary>
public sealed class GamepadManager : IDisposable
{
    private readonly GamepadDevice[] _devices =
    {
        new(0), new(1), new(2), new(3)
    };

    private readonly Dictionary<int, GamepadState?> _last = new()
    {
        [0] = null,
        [1] = null,
        [2] = null,
        [3] = null
    };

    private CancellationTokenSource? _cts;
    private Task? _loop;

    /// <summary>Polling interval in milliseconds. Default: 8ms (~125 Hz).</summary>
    public int PollIntervalMs { get; set; } = 8;

    /// <summary>Fired when a device transitions from disconnected to connected.</summary>
    public event Action<GamepadDevice>? DeviceConnected;

    /// <summary>Fired when a device transitions from connected to disconnected.</summary>
    public event Action<int /*index*/>? DeviceDisconnected;

    /// <summary>Fired whenever a device's packet number changes (state update).</summary>
    public event Action<GamepadDevice, GamepadState>? StateChanged;

    /// <summary>Access device by index (0..3).</summary>
    public GamepadDevice this[int index] => _devices[index];

    /// <summary>Returns currently connected devices.</summary>
    public IReadOnlyList<GamepadDevice> GetConnectedDevices()
    {
        var list = new List<GamepadDevice>(4);
        for (int i = 0; i < 4; i++)
            if (_devices[i].TryGetState(out _))
                list.Add(_devices[i]);
        return list;
    }

    /// <summary>Starts internal polling loop.</summary>
    public void Start()
    {
        if (_loop != null) return;
        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => PollLoopAsync(_cts.Token));
    }

    /// <summary>Stops internal polling loop.</summary>
    public void Stop()
    {
        try
        {
            _cts?.Cancel();
            _loop?.Wait();
        }
        catch { /* ignore */ }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            _loop = null;
        }
    }

    private async Task PollLoopAsync(CancellationToken ct)
    {
        var delay = TimeSpan.FromMilliseconds(PollIntervalMs);

        while (!ct.IsCancellationRequested)
        {
            for (int i = 0; i < 4; i++)
            {
                var dev = _devices[i];

                if (dev.TryGetState(out var state))
                {
                    // connected
                    var prev = _last[i];
                    if (prev is null)
                        DeviceConnected?.Invoke(dev);

                    // Notify only on packet number change (XInput increments when input changes)
                    if (prev is null || prev.PacketNumber != state.PacketNumber)
                    {
                        _last[i] = state;
                        dev.LastState = state;
                        StateChanged?.Invoke(dev, state);
                    }
                }
                else
                {
                    // disconnected
                    if (_last[i] is not null)
                    {
                        _last[i] = null;
                        dev.LastState = null;
                        DeviceDisconnected?.Invoke(i);
                    }
                }
            }

            try { await Task.Delay(delay, ct); }
            catch (TaskCanceledException) { break; }
        }
    }

    public void Dispose() => Stop();
}
