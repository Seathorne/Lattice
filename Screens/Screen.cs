using System;
using Lattice.Elements;
using Lattice.Input;

namespace Lattice.Screens;

public abstract class Screen : IAcceptsInput
{
    private Element? _root;

    public virtual Element Root => _root ??= Build();

    public bool AcceptsInput { get; set; }

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