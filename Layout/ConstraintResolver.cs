using System;
using System.Diagnostics;
using System.Reflection;
using Lattice.Elements;
using Lattice.Elements.Attributes;

namespace Lattice.Layout;

public static class ConstraintResolver
{
    public static SizeConstraint ResolveWidth(Type elementType, MemberInfo? declaration)
    {
        WidthAttribute? attribute = NearestWidth(elementType, declaration);

        switch (attribute)
        {
            case FixedWidthAttribute fixedWidth:
                if (fixedWidth.Width < 0)
                {
                    Trace.TraceWarning($"{nameof(FixedWidthAttribute)} value {fixedWidth.Width} is negative; using 0.");
                    return SizeConstraint.Fixed(0);
                }

                return SizeConstraint.Fixed(fixedWidth.Width);

            case FillWidthAttribute fillWidth:
                if (fillWidth.Minimum > fillWidth.Maximum)
                {
                    Trace.TraceWarning(
                        $"{nameof(FillWidthAttribute)} minimum {fillWidth.Minimum} exceeds "
                        + $"maximum {fillWidth.Maximum}; clamping to maximum.");

                    return SizeConstraint.Flexible(fillWidth.Maximum, fillWidth.Maximum);
                }

                return SizeConstraint.Flexible(fillWidth.Minimum, fillWidth.Maximum);

            default:
                return SizeConstraint.Fill;
        }
    }

    public static SizeConstraint ResolveHeight(Type elementType, MemberInfo? declaration)
    {
        HeightAttribute? attribute = NearestHeight(elementType, declaration);

        switch (attribute)
        {
            case FixedHeightAttribute fixedHeight:
                if (fixedHeight.Height < 0)
                {
                    Trace.TraceWarning($"{nameof(FixedHeightAttribute)} value {fixedHeight.Height} is negative; using 0.");
                    return SizeConstraint.Fixed(0);
                }

                return SizeConstraint.Fixed(fixedHeight.Height);

            case FillHeightAttribute fillHeight:
                if (fillHeight.Minimum > fillHeight.Maximum)
                {
                    Trace.TraceWarning(
                        $"{nameof(FillHeightAttribute)} minimum {fillHeight.Minimum} exceeds "
                        + $"maximum {fillHeight.Maximum}; clamping to maximum.");

                    return SizeConstraint.Flexible(fillHeight.Maximum, fillHeight.Maximum);
                }

                return SizeConstraint.Flexible(fillHeight.Minimum, fillHeight.Maximum);

            default:
                return SizeConstraint.Fill;
        }
    }

    public static bool ResolveClearBeforeRender(Type elementType, MemberInfo? declaration)
        => Nearest<ClearBeforeRenderAttribute>(elementType, declaration)?.ClearBeforeRender ?? false;

    private static WidthAttribute? NearestWidth(Type elementType, MemberInfo? declaration)
    {
        if (declaration is not null)
        {
            WidthAttribute[] onDeclaration = GetAttributes<WidthAttribute>(declaration, inherit: false);

            if (onDeclaration.Length > 0)
                return PreferFixed(onDeclaration, declaration);
        }

        WidthAttribute[] own = GetAttributes<WidthAttribute>(elementType, inherit: false);

        if (own.Length > 0)
            return PreferFixed(own, elementType);

        WidthAttribute[] inherited = GetAttributes<WidthAttribute>(elementType, inherit: true);

        return inherited.Length > 0
            ? PreferFixed(inherited, elementType)
            : null;
    }

    private static HeightAttribute? NearestHeight(Type elementType, MemberInfo? declaration)
    {
        if (declaration is not null)
        {
            HeightAttribute[] onDeclaration = GetAttributes<HeightAttribute>(declaration, inherit: false);

            if (onDeclaration.Length > 0)
                return PreferFixed(onDeclaration, declaration);
        }

        HeightAttribute[] own = GetAttributes<HeightAttribute>(elementType, inherit: false);

        if (own.Length > 0)
            return PreferFixed(own, elementType);

        HeightAttribute[] inherited = GetAttributes<HeightAttribute>(elementType, inherit: true);

        return inherited.Length > 0
            ? PreferFixed(inherited, elementType)
            : null;
    }

    private static TAttribute? Nearest<TAttribute>(Type elementType, MemberInfo? declaration)
        where TAttribute : Attribute
    {
        if (declaration is not null)
        {
            TAttribute[] onDeclaration = GetAttributes<TAttribute>(declaration, inherit: false);

            if (onDeclaration.Length > 0)
                return onDeclaration[0];
        }

        TAttribute[] own = GetAttributes<TAttribute>(elementType, inherit: false);

        if (own.Length > 0)
            return own[0];

        TAttribute[] inherited = GetAttributes<TAttribute>(elementType, inherit: true);

        return inherited.Length > 0 ? inherited[0] : null;
    }

    private static TAttribute PreferFixed<TAttribute>(TAttribute[] found, MemberInfo site)
        where TAttribute : Attribute
    {
        if (found.Length == 1)
            return found[0];

        foreach (TAttribute candidate in found)
        {
            if (candidate is FixedWidthAttribute or FixedHeightAttribute)
            {
                Trace.TraceWarning(
                    $"{site.Name} declares both {candidate.GetType().Name} and a fill "
                    + "constraint on the same axis; using the fixed one.");

                return candidate;
            }
        }

        Trace.TraceWarning(
            $"{site.Name} declares {found.Length} {typeof(TAttribute).Name} constraints; using the first.");

        return found[0];
    }

    private static TAttribute[] GetAttributes<TAttribute>(MemberInfo member, bool inherit)
        where TAttribute : Attribute
        => (TAttribute[])member.GetCustomAttributes(typeof(TAttribute), inherit);
}