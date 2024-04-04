using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace SatialInterfaces.Converters;

/// <summary>This converter converts an X Offset (from a scroll viewer) to a new X Offset using a threshold.</summary>
public class XOffsetThresholdConverter : IValueConverter
{
    /// <summary>
    /// Check if last value is set.
    /// </summary>
    private bool lastValueSet;

    /// <summary>
    /// Last value.
    /// </summary>
    private Vector lastValue;

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Vector v || parameter is not double threshold)
        {
            return value;
        }

        if (!lastValueSet)
        {
            lastValue = v;
            lastValueSet = true;
            return v;
        }

        var diff = v.X - lastValue.X;
        var absDiff = Math.Abs(diff);
        if (absDiff <= threshold)
        {
            v = new Vector(lastValue.X, v.Y);
            lastValue = v;
            return v;
        }

        lastValue = v;
        return v;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}