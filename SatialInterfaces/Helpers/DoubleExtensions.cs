using System;
using System.Diagnostics.CodeAnalysis;

namespace SatialInterfaces.Helpers;

/// <summary>
/// This class contains double extension methods.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Checks if the given value is zero, considering the double epsilon.
    /// </summary>
    /// <returns>True if the value is zero and false otherwise.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Extension Value")]
    public static bool IsZero(this double value)
    {
        return Math.Abs(value) < double.Epsilon;
    }

    /// <summary>
    /// Checks if the value is equal to the other number, considering the double epsilon.
    /// </summary>
    /// <param name="other">The number to check the value against.</param>
    /// <returns>True if the values are equal.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Extension Value")]
    public static bool IsEqual(this double value, double other)
    {
        return (value - other).IsZero();
    }

    /// <summary>
    /// Checks if the value is greater than the other number, considering the double epsilon.
    /// </summary>
    /// <param name="other">The number to compare the value to.</param>
    /// <returns>True if the <c>Double</c> is greater than the provided value.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Extension Value")]
    public static bool IsGreaterThan(this double value, double other)
    {
        return (value - other) > double.Epsilon;
    }

    /// <summary>
    /// Checks if the value is greater than or equal to the other number, considering the double epsilon.
    /// </summary>
    /// <param name="other">The number to compare the value to.</param>
    /// <returns>True if the value is greater than or equal to the other number.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Extension Value")]
    public static bool IsGreaterOrEqual(this double value, double other)
    {
        return value.IsGreaterThan(other) || value.IsEqual(other);
    }

    /// <summary>
    /// Checks if the value is less than the provided other value, considering the double epsilon.
    /// </summary>
    /// <param name="other">The number to compare the value to.</param>
    /// <returns>True if the value is less than the other number.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Extension Value")]
    public static bool IsLessThan(this double value, double other)
    {
        return (value - other) < -double.Epsilon;
    }

    /// <summary>
    /// Checks if the value is less than or equal to the other number, considering the double epsilon.
    /// </summary>
    /// <param name="other">The number to check against.</param>
    /// <returns>True if the value is less than or equal to the other number.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "Extension Value")]
    public static bool IsLessOrEqual(this double value, double other)
    {
        return value.IsLessThan(other) || value.IsEqual(other);
    }
}
