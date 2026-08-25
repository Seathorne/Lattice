using System;
using Lattice.Drawing;
using Lattice.Elements;
using Lattice.Input;

namespace Lattice.Screens;

public abstract class Screen : IDirtyable, IAcceptsInput
{
    private Element? _root;

    public virtual Element Root => _root ??= Build();

    public bool IsDirtyable { get; set; } = true;

    public bool AcceptsInput { get; set; } = true;

    public bool IsDirty { get; private set; }

    public void Invalidate()
    {
        if (!IsDirtyable)
            return;

        IsDirty = true;
    }

    public void ClearDirty()
        => IsDirty = false;

    public virtual bool HandleInput(ConsoleKeyInfo key, KeyEvent keyEvent)
        => false;

    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    protected abstract Element Build();
}