using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lattice.Elements;

namespace Lattice.Focus;

public sealed class FocusManager
{
    private static FocusManager? _current;

    private readonly List<IFocusable> _candidates = [];

    private int _index = -1;

    public static FocusManager Current
    {
        get
        {
            if (_current is null)
            {
                Trace.TraceWarning(
                    $"{nameof(FocusManager)}.{nameof(Current)} was read before "
                    + $"{nameof(SetCurrent)}; nothing is focusable.");

                _current = new FocusManager();
            }

            return _current;
        }
    }

    public IFocusable? Focused
        => _index >= 0 && _index < _candidates.Count && _candidates[_index].IsFocusable
            ? _candidates[_index]
            : null;

    public bool InnerScope { get; private set; }

    public int Count => _candidates.Count;

    public static void SetCurrent(FocusManager manager)
        => _current = manager ?? throw new ArgumentNullException(nameof(manager));

    public bool Focus(IFocusable target)
    {
        int index = _candidates.IndexOf(target);

        if (index < 0)
        {
            Trace.TraceWarning(
                $"{target.GetType().Name} is not a focus candidate; focus unchanged.");

            return false;
        }

        if (!target.IsFocusable)
        {
            Trace.TraceWarning(
                $"{target.GetType().Name} is not currently focusable; focus unchanged.");

            return false;
        }

        Move(index);

        return true;
    }

    internal void Load(Element root)
    {
        IFocusable? previous = Focused;

        _candidates.Clear();
        _index = -1;
        InnerScope = false;

        foreach (Element element in Flatten(root))
        {
            if (element is IFocusable focusable)
                _candidates.Add(focusable);
        }

        previous?.OnFocusLost();

        int first = Scan(-1, forward: true);

        if (first >= 0)
        {
            _index = first;
            Focused?.OnFocusGained();
        }
    }

    internal void Clear()
    {
        Focused?.OnFocusLost();

        _candidates.Clear();
        _index = -1;
        InnerScope = false;
    }

    internal void FocusNext()
        => Advance(forward: true);

    internal void FocusPrevious()
        => Advance(forward: false);

    internal void EnterInnerScope()
    {
        if (Focused is null)
        {
            Trace.TraceWarning($"{nameof(EnterInnerScope)} called with nothing focused; ignored.");
            return;
        }

        InnerScope = true;
    }

    internal void ExitInnerScope()
        => InnerScope = false;

    private void Advance(bool forward)
    {
        if (_candidates.Count == 0)
            return;

        if (InnerScope)
        {
            Trace.TraceWarning(
                $"Focus moved while inner scope was active; scope exited.");

            ExitInnerScope();
        }

        int next = Scan(_index, forward);

        if (next >= 0)
            Move(next);
    }

    private int Scan(int from, bool forward)
    {
        int count = _candidates.Count;

        if (count == 0)
            return -1;

        int step = forward ? 1 : -1;
        int start = from < 0 ? (forward ? -1 : 0) : from;

        for (int offset = 1; offset <= count; offset++)
        {
            int index = ((start + step * offset) % count + count) % count;

            if (_candidates[index].IsFocusable)
                return index;
        }

        return -1;
    }

    private void Move(int index)
    {
        IFocusable? previous = Focused;

        _index = index;

        if (ReferenceEquals(previous, Focused))
            return;

        previous?.OnFocusLost();
        Focused?.OnFocusGained();
    }

    private static IEnumerable<Element> Flatten(Element element)
    {
        yield return element;

        foreach (Element child in element.Children)
        {
            foreach (Element descendant in Flatten(child))
                yield return descendant;
        }
    }
}