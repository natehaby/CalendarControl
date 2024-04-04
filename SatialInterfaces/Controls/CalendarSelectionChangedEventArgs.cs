using Avalonia.Interactivity;

namespace SatialInterfaces.Controls.Calendar;

/// <summary>Expansion of the RoutedEventArgs class by providing a selected index;.</summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalendarSelectionChangedEventArgs"/> class, using the supplied routed event.
/// identifier.
/// </remarks>
/// <param name="routedEvent">The routed event identifier for this instance of the CalendarSelectionChangedEventArgs class.</param>
public class CalendarSelectionChangedEventArgs(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    /// <summary>
    /// Gets or sets the Index of the selected <see cref="CalendarControl.Items">Item</see>. -1 means deselected.
    /// </summary>
    /// <value>The selected index (zero-based) or -1 if deselected.</value>
    public int SelectedIndex { get; set; }

    /// <summary>
    /// Gets or sets an <c>Object</c> represeting the selected <see cref="CalendarControl.Items">Item</see>. Null means not selected.
    /// </summary>
    /// <value>The selected object or null if deselected.</value>
    public object? SelectedItem { get; set; }
}