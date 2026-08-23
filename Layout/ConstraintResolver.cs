using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Lattice.Elements;
using Lattice.Measure;

namespace Lattice.Layout;

public static class ConstraintResolver
{
    public static SizeConstraint Resolve(Type elementType, MemberInfo? declaration, Axis axis)
    {
        DimensionAttribute? attribute = NearestDimension(elementType, declaration, axis);

        return attribute switch
        {
            FixedDimensionAttribute fixedDimension => ToFixed(fixedDimension),
            AdaptiveDimensionAttribute adaptive => ToFlexible(adaptive, SizeMode.Adaptive),
            FlexibleDimensionAttribute flexible => ToFlexible(flexible, SizeMode.Fill),
            _ => SizeConstraint.Fill,
        };
    }

    public static BorderAttribute? ResolveBorder(Type elementType, MemberInfo? declaration)
        => Nearest<BorderAttribute>(elementType, declaration);

    public static bool ResolveIsVisible(Type elementType, MemberInfo? declaration)
        => Nearest<IsVisibleAttribute>(elementType, declaration)?.IsVisible ?? true;

    public static bool ResolveClearBeforeRender(Type elementType, MemberInfo? declaration)
        => Nearest<ClearBeforeRenderAttribute>(elementType, declaration)?.ClearBeforeRender ?? false;

    private static SizeConstraint ToFixed(FixedDimensionAttribute attribute)
    {
        if (attribute.Value >= 0)
            return SizeConstraint.Fixed(attribute.Value);

        Trace.TraceWarning(
            $"{attribute.GetType().Name} value {attribute.Value} is negative; using 0.");

        return SizeConstraint.Fixed(0);
    }

    private static SizeConstraint ToFlexible(FlexibleDimensionAttribute attribute, SizeMode mode)
    {
        int minimum = attribute.Minimum;
        int maximum = attribute.Maximum;

        if (minimum > maximum)
        {
            Trace.TraceWarning(
                $"{attribute.GetType().Name} minimum {minimum} exceeds "
                + $"maximum {maximum}; clamping to maximum.");

            minimum = maximum;
        }

        return mode == SizeMode.Adaptive
            ? SizeConstraint.AdaptiveWithin(minimum, maximum)
            : SizeConstraint.Flexible(minimum, maximum);
    }

    private static DimensionAttribute? NearestDimension(
        Type elementType, MemberInfo? declaration, Axis axis)
    {
        if (declaration is not null)
        {
            List<DimensionAttribute> onDeclaration =
                OnAxis(GetAttributes<DimensionAttribute>(declaration, inherit: false), axis);

            if (onDeclaration.Count > 0)
                return PreferFixed(onDeclaration, declaration, axis);
        }

        List<DimensionAttribute> own =
            OnAxis(GetAttributes<DimensionAttribute>(elementType, inherit: false), axis);

        if (own.Count > 0)
            return PreferFixed(own, elementType, axis);

        List<DimensionAttribute> inherited =
            OnAxis(GetAttributes<DimensionAttribute>(elementType, inherit: true), axis);

        return inherited.Count > 0
            ? PreferFixed(inherited, elementType, axis)
            : null;
    }

    private static List<DimensionAttribute> OnAxis(DimensionAttribute[] found, Axis axis)
    {
        List<DimensionAttribute> matches = [];

        foreach (DimensionAttribute attribute in found)
        {
            if (attribute.Axis == axis)
                matches.Add(attribute);
        }

        return matches;
    }

    private static DimensionAttribute PreferFixed(
        List<DimensionAttribute> found, MemberInfo site, Axis axis)
    {
        if (found.Count == 1)
            return found[0];

        foreach (DimensionAttribute candidate in found)
        {
            if (candidate is FixedDimensionAttribute)
            {
                Trace.TraceWarning(
                    $"{site.Name} declares {found.Count} {axis} constraints "
                    + $"including {candidate.GetType().Name}; using the fixed one.");

                return candidate;
            }
        }

        Trace.TraceWarning(
            $"{site.Name} declares {found.Count} {axis} constraints; using the first.");

        return found[0];
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

    private static TAttribute[] GetAttributes<TAttribute>(MemberInfo member, bool inherit)
        where TAttribute : Attribute
        => (TAttribute[])member.GetCustomAttributes(typeof(TAttribute), inherit);
}