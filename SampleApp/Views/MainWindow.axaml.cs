using System;
using System.ComponentModel;
using System.Globalization;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using SatialInterfaces.Controls.Calendar;

namespace SampleApp.Views;

public class ColorConverter : IValueConverter
{
    public static readonly ColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Status s)
            return Colors.Transparent;
        return s switch
        {
            Status.Information => Colors.Green,
            Status.Warning => Colors.Orange,
            Status.Error => Colors.Red,
            _ => Colors.Transparent
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public enum Status
{
    None,
    Information,
    Warning,
    Error
}

public class AppointmentViewModel
{
    public DateTime Begin { get; set; }
    public DateTime End { get; set; }
    public string Text { get; set; } = "";
    public Status Status { get; set; }
}

public partial class MainWindow : Window
{
    static readonly Random random = new();

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    CalendarControl? calendarControl;

    public AvaloniaList<AppointmentViewModel> Items
    {
        get => items;
        set => SetAndRaise(ItemsProperty, ref items, value);
    }
    public static readonly DirectProperty<MainWindow, AvaloniaList<AppointmentViewModel>> ItemsProperty = AvaloniaProperty.RegisterDirect<MainWindow, AvaloniaList<AppointmentViewModel>>(nameof(Items), o => o.Items, (o, v) => o.Items = v);
    AvaloniaList<AppointmentViewModel> items = [];

    public DateOnly SelectedDate
    {
        get => selectedDate;
        set => SetAndRaise(SelectedDateProperty, ref selectedDate, value);
    }
    public static readonly DirectProperty<MainWindow, DateOnly> SelectedDateProperty = AvaloniaProperty.RegisterDirect<MainWindow, DateOnly>(nameof(SelectedDate), o => o.SelectedDate, (o, v) => o.SelectedDate = v);
    DateOnly selectedDate = DateOnly.FromDateTime(DateTime.Now);

    public DisplayMode Mode
    {
        get => mode;
        set => SetAndRaise(ModeProperty, ref mode, value);
    }
    public static readonly DirectProperty<MainWindow, DisplayMode> ModeProperty = AvaloniaProperty.RegisterDirect<MainWindow, DisplayMode>(nameof(Mode), o => o.Mode, (o, v) => o.Mode = v);
    DisplayMode mode = DisplayMode.Day;

    public int Days
    {
        get => days;
        set => SetAndRaise(DaysProperty, ref days, value);
    }
    public static readonly DirectProperty<MainWindow, int> DaysProperty = AvaloniaProperty.RegisterDirect<MainWindow, int>(nameof(Days), o => o.Days, (o, v) => o.Days = v);
    int days = 3;

    public DisplayPosition SelectedDatePosition
    {
        get => selectedDatePosition;
        set => SetAndRaise(SelectedDatePositionProperty, ref selectedDatePosition, value);
    }
    public static readonly DirectProperty<MainWindow, DisplayPosition> SelectedDatePositionProperty = AvaloniaProperty.RegisterDirect<MainWindow, DisplayPosition>(nameof(SelectedDatePosition), o => o.SelectedDatePosition, (o, v) => o.SelectedDatePosition = v);
    DisplayPosition selectedDatePosition = DisplayPosition.Center;


    void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        calendarControl = this.FindControl<CalendarControl>("CalendarControl");
        Dispatcher.UIThread.Post(() => calendarControl?.Focus());
        //RandomCalendar();
    }

    void CalendarControlSelectionChanged(object? sender, CalendarSelectionChangedEventArgs e)
    {
        var info = this.FindControl<TextBlock>("Info");
        if (info == null)
            return;
        if (e.SelectedIndex >= 0)
        {
            var item = items[e.SelectedIndex];
            info.Text = item.Text + ": " + item.Begin.ToShortTimeString() + " - " + item.End.ToShortTimeString();
        }
        else
        {
            info.Text = "";
        }
    }

    void RandomButtonClick(object? sender, RoutedEventArgs e) => RandomCalendar();

    void PreviousButtonClick(object? sender, RoutedEventArgs e)
    {
        if (calendarControl == null)
            return;

        calendarControl.SelectedDate = calendarControl.Mode switch
        {
            DisplayMode.Day => calendarControl.SelectedDate.AddDays(-1),
            DisplayMode.Week => calendarControl.SelectedDate.AddDays(-7),
            DisplayMode.WorkWeek => calendarControl.SelectedDate.AddDays(-7),
            _ => throw new Exception("Invalid DisplayMode")
        };
    }

    void ThisWeekButtonClick(object? sender, RoutedEventArgs e)
    {
        if (calendarControl == null)
            return;
        calendarControl.SelectedDate = DateOnly.FromDateTime(DateTime.Now);
    }

    void NextButtonClick(object? sender, RoutedEventArgs e)
    {
        if (calendarControl == null)
            return;

        calendarControl.SelectedDate = calendarControl.Mode switch
        {
            DisplayMode.Day => calendarControl.SelectedDate.AddDays(1),
            DisplayMode.Week => calendarControl.SelectedDate.AddDays(7),
            DisplayMode.WorkWeek => calendarControl.SelectedDate.AddDays(7),
            _ => throw new Exception("Invalid DisplayMode")
        };
    }

    void NewButtonClick(object? sender, RoutedEventArgs e)
    {
        if (calendarControl == null)
            return;
        var begin = GetRandomDateTime(DateTime.Now.AddDays(-2), DateTime.Now.AddDays(6));
        var end = begin.AddHours(random.NextDouble() * 8);

        var item = new AppointmentViewModel
        {
            Begin = begin,
            End = end,
            Text = $"New Appointment {items.Count}",
            Status = GetRandomStatus()
        };

        items.Add(item);
        calendarControl.ScrollIntoView(item);
    }

    /// <summary>
    /// Calculates the start date based on the current mode and selected date.
    /// </summary>
    /// <returns>The first date to be displayed</returns>
    public DateOnly StartDate()
    {
        DayOfWeek FirstDayOfWeek = DayOfWeek.Sunday;

        if (Mode == DisplayMode.Day && Days == 1)
            return SelectedDate;

        if (Mode == DisplayMode.Week)
        {
            if (SelectedDate.DayOfWeek == FirstDayOfWeek)
                return SelectedDate;
            int x = -(int)SelectedDate.DayOfWeek + (int)FirstDayOfWeek;
            return SelectedDate.AddDays(x);
        }

        if (Mode == DisplayMode.WorkWeek)
        {
            if (SelectedDate.DayOfWeek == FirstDayOfWeek)
                return SelectedDate;
            var start = SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek + (int)FirstDayOfWeek);
            if (start.DayOfWeek == DayOfWeek.Sunday)
                return start.AddDays(1);

            return start;
        }

        if (Days > 10)
            throw new ArgumentException($"Days cannot be greater than 10");

        if (SelectedDatePosition == DisplayPosition.Center)
        {
            var start = SelectedDate.AddDays((-Days / 2) + 1);
            return start;
        }

        if (SelectedDatePosition == DisplayPosition.Right)
        {
            return SelectedDate.AddDays((-Days) + 1);
        }

        // Only DisplayPosition.Left should be remaining.
        return SelectedDate;
    }

    /// <summary>
    /// Randomly populates the calendar with appointments.
    /// </summary>
    void RandomCalendar()
    {
        if (calendarControl == null)
            return;
        calendarControl.SelectedDate = DateOnly.FromDateTime(DateTime.Now);
        var beginOfWeek = StartDate().ToDateTime(TimeOnly.MinValue);

        items.Clear();
        const int weeks = 1;
        const int heads = weeks / 2;
        const int tails = weeks - heads;
        for (var i = 0; i < 24 * weeks; i++)
        {
            var begin = GetRandom(-168 * heads * 4, 168 * tails * 4) / 4.0d;
            var length = GetWeightedRandom(0.5f, 8f, 0.5f);

            var item = new AppointmentViewModel
            {
                Begin = beginOfWeek.AddHours(begin),
                End = beginOfWeek.AddHours(begin + length),
                Text = $"Appointment {i}",
                Status = GetRandomStatus()
            };
            items.Add(item);
        }

        calendarControl.SelectedIndex = 0;
    }

    static double GetRandom(double minVal, double maxVal)
    {
        double rand = random.NextDouble();
        return minVal + (rand * (maxVal - minVal));
    }

    static double GetWeightedRandom(double minVal, double maxVal, double weight)
    {
        double rand = random.NextDouble();
        double weightedRandom = -Math.Log(1 - rand) / weight;
        return weightedRandom;
    }

    static DateTime GetRandomDateTime(DateTime startDate, DateTime endDate)
    {
        TimeSpan timeSpan = endDate - startDate;
        TimeSpan randomSpan = new TimeSpan((long)(random.NextDouble() * timeSpan.Ticks));
        return startDate + randomSpan;
    }

    static Status GetRandomStatus() => (Status)GetRandom((float)Status.None, (float)Status.Error);
}