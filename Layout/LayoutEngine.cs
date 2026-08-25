using System;
using System.Collections.Generic;
using System.Reflection;
using Lattice.Drawing;
using Lattice.Elements;
using Lattice.Measure;
using Lattice.Screens;
using Lattice.Text;

namespace Lattice.Layout;

public sealed class LayoutEngine
{
    private const BindingFlags DeclarationFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private readonly Dictionary<Element, MemberInfo> _declarations = [];

    public IReadOnlyList<DrawSurface> Layout(Screen screen, Rectangle bounds)
    {
        MapDeclarations(screen);

        List<DrawSurface> surfaces = [];
        Place(screen.Root, bounds, surfaces);

        return surfaces;
    }

    private void MapDeclarations(Screen screen)
    {
        _declarations.Clear();

        Type screenType = screen.GetType();

        foreach (FieldInfo field in screenType.GetFields(DeclarationFlags))
        {
            if (field.GetValue(screen) is Element element)
                _declarations[element] = field;
        }

        foreach (PropertyInfo property in screenType.GetProperties(DeclarationFlags))
        {
            if (property.GetIndexParameters().Length > 0)
                continue;

            if (property.GetValue(screen) is Element element && !_declarations.ContainsKey(element))
                _declarations[element] = property;
        }
    }

    private MemberInfo? DeclarationOf(Element element)
        => _declarations.TryGetValue(element, out MemberInfo? found) ? found : null;

    private void Place(Element element, Rectangle allocated, List<DrawSurface> surfaces)
    {
        Type type = element.GetType();
        MemberInfo? declaration = DeclarationOf(element);

        if (!(element.IsVisible ?? ConstraintResolver.ResolveIsVisible(type, declaration)))
            return;

        DrawSurface surface = new(allocated);

        if (ConstraintResolver.ResolveClearBeforeRender(type, declaration))
            surface.Clear();

        Rectangle content = allocated;
        BorderAttribute? border = ConstraintResolver.ResolveBorder(type, declaration);

        if (border is not null)
        {
            BorderGlyphSet glyphs = BorderGlyphs.GetBorderGlyphs(border.Border);
            int thickness = Math.Max(1, WidthTable.Current.Measure(glyphs.Vertical));

            surface.Frame(border.Border);

            content = new Rectangle(
                allocated.Left + thickness,
                allocated.Top + 1,
                Math.Max(0, allocated.Width - thickness * 2),
                Math.Max(0, allocated.Height - 2));
        }

        element.Render(surface);
        surfaces.Add(surface);

        if (element.Children.Count == 0 || content.Width <= 0 || content.Height <= 0)
            return;

        foreach ((Element child, Rectangle cell) in Arrange(element, content))
            Place(child, cell, surfaces);
    }

    private IEnumerable<(Element Child, Rectangle Cell)> Arrange(Element parent, Rectangle content)
    {
        IReadOnlyList<Element> children = parent.Children;

        int columns = parent is GridElement grid
            ? Math.Max(1, grid.Columns)
            : Math.Max(1, children.Count);

        int rows = parent is GridElement gridRows
            ? Math.Max(1, gridRows.Rows)
            : 1;

        int[] columnWidths = Distribute(children, columns, content.Width, Axis.Width);
        int[] rowHeights = Distribute(children, columns, content.Height, Axis.Height, rows);

        for (int index = 0; index < children.Count; index++)
        {
            int column = index % columns;
            int row = index / columns;

            if (row >= rows)
                yield break;

            int x = content.Left;

            for (int i = 0; i < column; i++)
                x += columnWidths[i];

            int y = content.Top;

            for (int i = 0; i < row; i++)
                y += rowHeights[i];

            yield return (children[index], new Rectangle(x, y, columnWidths[column], rowHeights[row]));
        }
    }

    private int[] Distribute(
        IReadOnlyList<Element> children, int columns, int available, Axis axis, int rows = 0)
    {
        int trackCount = axis == Axis.Width ? columns : Math.Max(1, rows);
        int[] sizes = new int[trackCount];
        SizeConstraint[] constraints = new SizeConstraint[trackCount];
        bool[] isFlexible = new bool[trackCount];

        for (int track = 0; track < trackCount; track++)
            constraints[track] = SizeConstraint.Fill;

        for (int index = 0; index < children.Count; index++)
        {
            int track = axis == Axis.Width ? index % columns : index / columns;

            if (track >= trackCount)
                continue;

            Element child = children[index];

            SizeConstraint constraint = ConstraintResolver.Resolve(
                child.GetType(), DeclarationOf(child), axis);

            switch (constraint.Mode)
            {
                case SizeMode.Fixed:
                    sizes[track] = Math.Max(sizes[track], constraint.Clamp(0));
                    break;

                case SizeMode.Adaptive:
                    Rectangle intrinsic = child.MeasureAdaptive();
                    int measured = axis == Axis.Width ? intrinsic.Width : intrinsic.Height;
                    sizes[track] = Math.Max(sizes[track], constraint.Clamp(measured));
                    break;

                default:
                    isFlexible[track] = true;
                    constraints[track] = constraint;
                    break;
            }
        }

        int consumed = 0;
        int flexibleCount = 0;

        for (int track = 0; track < trackCount; track++)
        {
            if (isFlexible[track])
                flexibleCount++;
            else
                consumed += sizes[track];
        }

        if (flexibleCount == 0)
            return sizes;

        int remaining = Math.Max(0, available - consumed);
        int share = remaining / flexibleCount;
        int leftover = remaining % flexibleCount;

        for (int track = 0; track < trackCount; track++)
        {
            if (!isFlexible[track])
                continue;

            int size = share;

            if (leftover > 0)
            {
                size++;
                leftover--;
            }

            sizes[track] = constraints[track].Clamp(size);
        }

        return sizes;
    }
}