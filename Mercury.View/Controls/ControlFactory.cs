using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace Mercury.View;

/// <summary>
/// This class contains control factory methods.
/// </summary>
public static class ControlFactory
{
    /// <summary>
    /// Created a coloumn
    /// </summary>
    /// <returns>Created column</returns>
   public static Grid CreateColumn() => new();

    /// <summary>
    /// Creates a background cell
    /// </summary>
    /// <returns>Created cell</returns>
    public static Border CreateHourCell() => new HourCell();

    /// <summary>
    /// Creates an appointment
    /// </summary>
    /// <param name="text">Text to add to appointment</param>
    /// <param name="color">Color to add to appointment</param>
    /// <param name="index">Index to set to appointment</param>
    /// <returns>Created appointment</returns>
    public static Border CreateAppointment(DateTime begin, string text, Color color, int index)
    {
        var border = new AppointmentControl { Index = index };
        if (color != Colors.Transparent)
        {
            border.Background = new SolidColorBrush(color);
        }

        var textBlock = new TextBlock { Text = begin.ToShortTimeString() + Environment.NewLine + text };
        var grid = new Grid();
        grid.Children.Add(new Border());
        grid.Children.Add(textBlock);

        border.Child = grid;
        return border;
    }

    public static Grid CreateGrid(int columnCount)
    {
        var grid = new Grid();
        var columnDefinitions = new ColumnDefinitions();
        for (var i = 0; i < columnCount; i++)
        {
            columnDefinitions.Add(new ColumnDefinition(1.0d, GridUnitType.Star));
            grid.Children.Add(new Grid());
        }
        grid.ColumnDefinitions = columnDefinitions;
        for (var i = 0; i < columnCount; i++)
            Grid.SetColumn(grid.Children[i] as Control, i);
        return grid;
    }
}