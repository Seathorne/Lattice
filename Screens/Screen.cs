using Lattice.Elements;

namespace Lattice.Screens;

public abstract class Screen
{
    private Element? _root;

    public virtual Element Root => _root ??= Build();

    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    protected abstract Element Build();
}