using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lattice.Drawing;
using Lattice.Measure;

namespace Lattice.Elements;

[IsVisible(true)]
public abstract class Element
{
    private readonly List<Element> _children = [];

    private protected Element() { }

    public Guid Id { get; } = Guid.NewGuid();

    public bool? IsVisible { get; set; }

    public SizeConstraint? Width { get; set; }

    public SizeConstraint? Height { get; set; }

    public Border? Border { get; set; }

    public bool? ClearBeforeRender { get; set; }

    public Element? Parent { get; private set; }

    public IReadOnlyList<Element> Children => _children;

    public void Hide() => IsVisible = false;

    public void Show() => IsVisible = true;

    public void AddChild(Element child)
    {
        if (_children.Contains(child))
        {
            Trace.TraceWarning($"Element {this} already contains child {child}; nothing added.");
            return;
        }

        if (child.Parent is Element parent && parent.Children.Contains(child))
        {
            Trace.TraceWarning($"Removing child {child} from former parent {parent}.");
            parent.RemoveChild(child);
        }

        child.Parent = this;
        _children.Add(child);
        RaiseChanged(new ChildAddedEventArgs(child));
    }

    public void RemoveChild(Element child)
    {
        if (_children.Remove(child) == false)
        {
            Trace.TraceWarning($"Element {this} does not contain child {child}; nothing removed.");
            return;
        }

        child.Parent = null;
        RaiseChanged(new ChildRemovedEventArgs(child));
    }

    public abstract void Render(bool isFocused, DrawSurface drawSurface);

    public virtual Rectangle MeasureAdaptive()
        => new(0, 0, 0, 0);

    public override string ToString() => $"{GetType().Name}({Id})";

    protected void RaiseChanged(ElementChangedEventArgs e) => Changed?.Invoke(this, e);

    public event EventHandler<ElementChangedEventArgs>? Changed;
}