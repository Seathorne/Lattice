using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lattice.Elements;
using Lattice.Focus;
using Lattice.Screens;

namespace Lattice.Input;

public sealed class InputHandler
{
    private const int PollIntervalMs = 15;

    private static readonly ConsoleKey[] PolledKeys =
    [
        ConsoleKey.LeftArrow,
        ConsoleKey.RightArrow,
        ConsoleKey.UpArrow,
        ConsoleKey.DownArrow,
    ];

    private readonly BlockingCollection<InputEvent> _queue = new();

    private readonly Dictionary<ConsoleKey, KeyState> _held = [];

    private readonly Stopwatch _clock = Stopwatch.StartNew();

    private Thread? _reader;

    private Thread? _poller;

    private volatile bool _running;

    public bool AllowRepeat { get; set; } = true;

    public RepeatRate Rate { get; set; } = RepeatRate.Normal;

    public bool ExitRequested { get; private set; }

    public void Start()
    {
        if (_running)
            return;

        _running = true;

        _reader = new Thread(Read)
        {
            IsBackground = true,
            Name = "Lattice input reader",
        };

        _poller = new Thread(Poll)
        {
            IsBackground = true,
            Name = "Lattice input poller",
        };

        _reader.Start();
        _poller.Start();
    }

    public void Stop()
    {
        _running = false;
        _queue.CompleteAdding();
    }

    public bool TryTake(out InputEvent inputEvent, int timeoutMs)
    {
        inputEvent = default;

        try
        {
            return _queue.TryTake(out inputEvent, timeoutMs);
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public void Route(InputEvent inputEvent, Screen screen)
    {
        ConsoleKeyInfo key = inputEvent.Key;
        Trace.TraceWarning($"route {key.Key} {inputEvent.Event}");

        if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            ExitRequested = true;
            return;
        }

        if (key.Key == ConsoleKey.Escape && inputEvent.Event == KeyEvent.Pressed)
        {
            if (FocusManager.Current.InnerScope)
            {
                FocusManager.Current.ExitInnerScope();
                return;
            }

            ExitRequested = true;
            return;
        }

        if (FocusManager.Current.InnerScope)
        {
            if (Deliver(inputEvent))
                return;

            screen.HandleInput(key, inputEvent.Event);
            return;
        }

        if (key.Key == ConsoleKey.Tab && inputEvent.Event == KeyEvent.Pressed)
        {
            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                FocusManager.Current.FocusPrevious();
            else
                FocusManager.Current.FocusNext();

            return;
        }

        if (Deliver(inputEvent))
            return;

        if (screen.HandleInput(key, inputEvent.Event))
            return;

        if (inputEvent.Event != KeyEvent.Pressed && inputEvent.Event != KeyEvent.Held)
            return;

        switch (key.Key)
        {
            case ConsoleKey.RightArrow:
            case ConsoleKey.DownArrow:
                FocusManager.Current.FocusNext();
                break;

            case ConsoleKey.LeftArrow:
            case ConsoleKey.UpArrow:
                FocusManager.Current.FocusPrevious();
                break;
        }
    }

    private static bool Deliver(InputEvent inputEvent)
        => FocusManager.Current.Focused is IAcceptsInput target
        && target.AcceptsInput
        && target.HandleInput(inputEvent.Key, inputEvent.Event);

    private static bool IsPolled(ConsoleKey key)
    {
        foreach (ConsoleKey polled in PolledKeys)
        {
            if (polled == key)
                return true;
        }

        return false;
    }

    private void Read()
    {
        while (_running)
        {
            ConsoleKeyInfo key;

            try
            {
                key = System.Console.ReadKey(intercept: true);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            if (!_running)
                return;

            Trace.TraceWarning($"reader saw {key.Key} ({(int)key.Key})");

            if (IsPolled(key.Key))
                continue;

            Enqueue(new InputEvent(key, KeyEvent.Pressed));
        }
    }

    private void Poll()
    {
        while (_running)
        {
            long now = _clock.ElapsedMilliseconds;

            foreach (ConsoleKey key in PolledKeys)
            {
                bool down = NativeMethods.IsHeld((int)key);

                if (!_held.TryGetValue(key, out KeyState state))
                {
                    state = new KeyState();
                    _held[key] = state;
                }

                if (down && !state.IsDown)
                {
                    state.IsDown = true;
                    state.PressedAt = now;
                    state.LastRepeatAt = now;

                    Enqueue(Compose(key, KeyEvent.Pressed));
                }
                else if (down && state.IsDown)
                {
                    if (!AllowRepeat)
                        continue;

                    (int initialDelay, int interval) = Timing(Rate);

                    if (now - state.PressedAt < initialDelay)
                        continue;

                    if (now - state.LastRepeatAt < interval)
                        continue;

                    state.LastRepeatAt = now;

                    Enqueue(Compose(key, KeyEvent.Held));
                }
                else if (!down && state.IsDown)
                {
                    state.IsDown = false;

                    Enqueue(Compose(key, KeyEvent.Released));
                }
            }

            Thread.Sleep(PollIntervalMs);
        }
    }

    private void Enqueue(InputEvent inputEvent)
    {
        if (!_running)
            return;

        try
        {
            _queue.Add(inputEvent);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static InputEvent Compose(ConsoleKey key, KeyEvent keyEvent)
    {
        Trace.TraceWarning($"poller emitting {key} ({(int)key}) {keyEvent}");

        bool shift = NativeMethods.IsHeld(0x10);
        bool control = NativeMethods.IsHeld(0x11);
        bool alt = NativeMethods.IsHeld(0x12);

        ConsoleKeyInfo info = new('\0', key, shift, alt, control);

        return new InputEvent(info, keyEvent);
    }

    private static (int InitialDelay, int Interval) Timing(RepeatRate rate)
        => rate switch
        {
            RepeatRate.Slow => (500, 200),
            RepeatRate.Fast => (150, 50),
            _ => (300, 100),
        };

    private sealed class KeyState
    {
        public bool IsDown { get; set; }

        public long PressedAt { get; set; }

        public long LastRepeatAt { get; set; }
    }
}