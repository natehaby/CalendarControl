using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using SatialInterfaces.Factories;
using SatialInterfaces.Helpers;

namespace SatialInterfaces.Controls.Calendar;

/// <summary>This class represents a calendar control (week view).</summary>
public partial class CalendarControl : ContentControl
{
    /// <summary>Auto scroll to selected item property.</summary>
    public static readonly DirectProperty<CalendarControl, bool> AutoScrollToSelectedItemProperty = AvaloniaProperty.RegisterDirect<CalendarControl, bool>(nameof(AutoScrollToSelectedItem), o => o.AutoScrollToSelectedItem, (o, v) => o.AutoScrollToSelectedItem = v);

    /// <summary>Allow delete property.</summary>
    public static readonly DirectProperty<CalendarControl, bool> AllowDeleteProperty = AvaloniaProperty.RegisterDirect<CalendarControl, bool>(nameof(AllowDelete), o => o.AllowDelete, (o, v) => o.AllowDelete = v);

    /// <summary>Margin around the appointment group property.</summary>
    public static readonly StyledProperty<Thickness> AppointmentGroupMarginProperty = AvaloniaProperty.Register<CalendarControl, Thickness>(nameof(AppointmentGroupMargin));

    /// <summary>First day of the week property.</summary>
    public static readonly StyledProperty<DayOfWeek> FirstDayOfWeekProperty = AvaloniaProperty.Register<CalendarControl, DayOfWeek>(nameof(FirstDayOfWeek), DateTimeHelper.GetCurrentDateFormat().FirstDayOfWeek);

    /// <summary>Items property.</summary>
    public static readonly DirectProperty<CalendarControl, IEnumerable> ItemsProperty = AvaloniaProperty.RegisterDirect<CalendarControl, IEnumerable>(nameof(Items), o => o.Items, (o, v) => o.Items = v);

    /// <summary>The selected date.</summary>
    public static readonly DirectProperty<CalendarControl, DateOnly> SelectedDateProperty = AvaloniaProperty.RegisterDirect<CalendarControl, DateOnly>(nameof(SelectedDate), o => o.SelectedDate, (o, v) => o.SelectedDate = v);

    /// <summary>The begin of the day property.</summary>
    public static readonly DirectProperty<CalendarControl, TimeSpan> BeginOfTheDayProperty = AvaloniaProperty.RegisterDirect<CalendarControl, TimeSpan>(nameof(BeginOfTheDay), o => o.BeginOfTheDay, (o, v) => o.BeginOfTheDay = v);

    /// <summary>The end of the day property.</summary>
    public static readonly DirectProperty<CalendarControl, TimeSpan> EndOfTheDayProperty = AvaloniaProperty.RegisterDirect<CalendarControl, TimeSpan>(nameof(EndOfTheDay), o => o.EndOfTheDay, (o, v) => o.EndOfTheDay = v);

    /// <summary>Item template.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty = AvaloniaProperty.Register<CalendarControl, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>The selected index property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<CalendarControl, int>(nameof(SelectedIndex), -1);

    /// <summary>The selected item property.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty = AvaloniaProperty.Register<CalendarControl, object?>(nameof(SelectedItem));

    /// <summary>Use default items template property.</summary>
    public static readonly DirectProperty<CalendarControl, bool> UseDefaultItemsTemplateProperty = AvaloniaProperty.RegisterDirect<CalendarControl, bool>(nameof(UseDefaultItemsTemplate), o => o.UseDefaultItemsTemplate, (o, v) => o.UseDefaultItemsTemplate = v);

    /// <summary>The selection changed event.</summary>
    public static readonly RoutedEvent<CalendarSelectionChangedEventArgs> SelectionChangedEvent = RoutedEvent.Register<CalendarControl, CalendarSelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

    /// <summary>Days to view.</summary>
    public static readonly DirectProperty<CalendarControl, DisplayMode> ModeProperty = AvaloniaProperty.RegisterDirect<CalendarControl, DisplayMode>(nameof(Mode), o => o.Mode, (o, v) => o.Mode = v);

    /// <summary>Position of selected date. Only works with Day View.</summary>
    /// <remarks>When using Center with Day Mode and an even number of Days, the selected date will be left of center.</remarks>
    public static readonly DirectProperty<CalendarControl, DisplayPosition> SelectedDatePositionProperty = AvaloniaProperty.RegisterDirect<CalendarControl, DisplayPosition>(nameof(SelectedDatePosition), o => o.SelectedDatePosition, (o, v) => o.SelectedDatePosition = v);

    /// <summary>Days to show in Day View.</summary>
    public static readonly DirectProperty<CalendarControl, int> DaysProperty = AvaloniaProperty.RegisterDirect<CalendarControl, int>(nameof(Days), o => o.Days, (o, v) => o.Days = v);

    /// <summary>
    /// The maximum number of days that can be displayed when <see cref="Mode" is set to Day./>.
    /// </summary>
    private const int MaxDays = 10;

    /// <summary>Hours per day.</summary>
    private const int HoursPerDay = 24;

    /// <summary>Items grid.</summary>
    private readonly Grid? itemsGrid;

    /// <summary>Scroll viewer main.</summary>
    private readonly ScrollViewer? scrollViewerMain;

    /// <summary>scrollable grid.</summary>
    private readonly Grid? scrollableGrid;

    /// <summary>Week grid.</summary>
    private readonly Grid? weekGrid;

    /// <summary>Day grid.</summary>
    private readonly Grid? dayGrid;

    /// <summary>Hour grid.</summary>
    private readonly Grid? hourGrid;

    /// <summary>Auto scroll to selected item.</summary>
    private bool autoScrollToSelectedItem;

    /// <summary>Allow delete.</summary>
    private bool allowDelete;

    /// <summary>Days.</summary>
    private int days = 1;

    /// <summary>Selected Date.</summary>
    private DateOnly selectedDate = DateOnly.FromDateTime(DateTime.Now);

    /// <summary>Sets the display mode. <see cref="DisplayMode"/>.</summary>
    private DisplayMode mode = DisplayMode.Week;

    /// <summary>Selected date position <see cref="DisplayPosition"/>.</summary>
    private DisplayPosition selectedDatePosition = DisplayPosition.Left;

    /// <summary>Begin of the day.</summary>
    private TimeSpan beginOfTheDay = new(0, 0, 0);

    /// <summary>End of the day.</summary>
    private TimeSpan endOfTheDay = new(0, 0, 0);

    /// <summary>Items.</summary>
    private IEnumerable items = new AvaloniaList<object>();

    /// <summary>Use default items template.</summary>
    private bool useDefaultItemsTemplate = true;

    /// <summary>State of the left mouse button.</summary>
    private bool leftButtonDown;

    /// <summary>Skip handling the items changed event flag.</summary>
    private bool skipItemsChanged;

    /// <summary>Skip handling the selected index changed event flag.</summary>
    private bool skipSelectedIndexChanged;

    /// <summary>Skip handling the selected item changed event flag.</summary>
    private bool skipSelectedItemChanged;

    /// <summary>Collection changed subscription.</summary>
    private IDisposable? collectionChangeSubscription;

    /// <summary>Initializes static members of the <see cref="CalendarControl" /> class.</summary>
    static CalendarControl()
    {
        FocusableProperty.OverrideDefaultValue<CalendarControl>(true);

        ItemsProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.ItemsChanged(e));
        BeginOfTheDayProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.BeginOfTheDayChanged(e));
        SelectedDateProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.SelectedDateChanged(e));
        EndOfTheDayProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.EndOfTheDayChanged(e));
        SelectedIndexProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.SelectedIndexChanged(e));
        SelectedItemProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.SelectedItemChanged(e));
        ModeProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.ModeChanged());
        DaysProperty.Changed.AddClassHandler<CalendarControl>((x, e) => x.DaysChanged(e));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarControl" /> class.
    /// </summary>
    public CalendarControl()
    {
        AvaloniaXamlLoader.Load(this);
        itemsGrid = this.FindControl<Grid>("ItemsGrid");
        scrollViewerMain = this.FindControl<ScrollViewer>("MainScrollViewer");
        scrollableGrid = this.FindControl<Grid>("ScrollableGrid");
        weekGrid = this.FindControl<Grid>("WeekGrid");
        dayGrid = this.FindControl<Grid>("DayGrid");
        hourGrid = this.FindControl<Grid>("HourGrid");

        scrollViewerMain?.GetObservable(BoundsProperty).Subscribe(OnScrollViewerBoundsChanged);

#if DEBUG
        // Sanity checks
        if (itemsGrid is null)
        {
            throw new NotImplementedException("ItemsGrid not found");
        }

        if (scrollViewerMain is null)
        {
            throw new NotImplementedException("ScrollViewerMain not found");
        }

        if (scrollableGrid is null)
        {
            throw new NotImplementedException("ScrollableGrid not found");
        }

        if (weekGrid is null)
        {
            throw new NotImplementedException("WeekGrid not found");
        }

        if (dayGrid is null)
        {
            throw new NotImplementedException("DayGrid not found");
        }

        if (hourGrid is null)
        {
            throw new NotImplementedException("HourGrid not found");
        }
#endif

        DrawCalendar();
        UpdateItems(Items, SelectedIndex);
    }

    /// <summary>Occurs when selection changed</summary>
    public event EventHandler<CalendarSelectionChangedEventArgs> SelectionChanged { add => AddHandler(SelectionChangedEvent, value); remove => RemoveHandler(SelectionChangedEvent, value); }

    /// <summary>Gets or sets a value indicating whether the calender should auto scroll to to the <see cref="SelectedItem">SelectedItem</see>.</summary>
    public bool AutoScrollToSelectedItem { get => autoScrollToSelectedItem; set => SetAndRaise(AutoScrollToSelectedItemProperty, ref autoScrollToSelectedItem, value); }

    /// <summary>Gets or sets a value indicating whether to allow deletion of an <see cref="Item">Item</see>.</summary>
    public bool AllowDelete { get => allowDelete; set => SetAndRaise(AllowDeleteProperty, ref allowDelete, value); }

    /// <summary>Gets or sets the margin around the appointment group.</summary>
    public Thickness AppointmentGroupMargin { get => GetValue(AppointmentGroupMarginProperty); set => SetValue(AppointmentGroupMarginProperty, value); }

    /// <summary>Gets or sets the first day of the week.</summary>
    public DayOfWeek FirstDayOfWeek { get => GetValue(FirstDayOfWeekProperty); set => SetValue(FirstDayOfWeekProperty, value); }

    /// <summary>Gets or sets the Appointment Items displayed on the calendar.</summary>
    public IEnumerable Items { get => items; set => SetAndRaise(ItemsProperty, ref items, value); }

    /// <summary>Begin of the day property.</summary>
    public TimeSpan BeginOfTheDay { get => beginOfTheDay; set => SetAndRaise(BeginOfTheDayProperty, ref beginOfTheDay, value); }

    /// <summary>End of the day property.</summary>
    public TimeSpan EndOfTheDay { get => endOfTheDay; set => SetAndRaise(EndOfTheDayProperty, ref endOfTheDay, value); }

    /// <summary>Gets or sets the Item Template used to display the <see cref="Items">Item</see>.</summary>
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }

    /// <summary>Gets or sets the index of the currently selected <see cref="Items">Item</see>.</summary>
    public int SelectedIndex { get => GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }

    /// <summary>Gets or sets the currently selected appointment <see cref="Items">Item</see>.</summary>
    public object? SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

    /// <summary>Gets or sets the date selected in the calendar.</summary>
    public DateOnly SelectedDate { get => selectedDate; set => SetAndRaise(SelectedDateProperty, ref selectedDate, value); }

    /// <summary>Gets or sets a value indicating whether the default item template should be used.</summary>
    public bool UseDefaultItemsTemplate { get => useDefaultItemsTemplate; set => SetAndRaise(UseDefaultItemsTemplateProperty, ref useDefaultItemsTemplate, value); }

    /// <summary>Gets or sets the display mode.</summary>
    public DisplayMode Mode { get => mode; set => SetAndRaise(ModeProperty, ref mode, value); }

    /// <summary>Gets or sets the Position to show the <see cref="SelectedDate">SelectedDate</see> in <c>Day</c> <see cref="Mode">Mode</see>.</summary>
    public DisplayPosition SelectedDatePosition { get => selectedDatePosition; set => SetAndRaise(SelectedDatePositionProperty, ref selectedDatePosition, value); }

    /// <summary>Gets or sets the number of days to display in Day Mode.</summary>
    public int Days { get => days; set => SetAndRaise(DaysProperty, ref days, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CalendarControl);

    /// <summary>
    /// Gets the number of days to show based on the current <see cref="Mode">Mode</see> and <see cref="Days">Days</see>.
    /// </summary>
    private int DaysToShow => Mode switch
    {
        DisplayMode.Day => Days,
        DisplayMode.WorkWeek => 5,
        DisplayMode.Week => 7,
        _ => throw new ArgumentException("Invalid mode")
    };

    /// <summary>
    /// Gets the number of days to move when clicking the next or previous button.
    /// </summary>
    /// <remarks><c>Week</c> and <c>WorkWeek</c> <see cref="Mode">Modes</see> return 7. <c>Days</c> <see cref="Mode">Mode</see> returns <see cref="Days">Days</see>.</remarks>
    private int DaysToMove => Mode switch
    {
        DisplayMode.Day => Days,
        DisplayMode.WorkWeek => 7,
        DisplayMode.Week => 7,
        _ => throw new ArgumentException("Invalid mode")
    };

    /// <summary>
    /// Calculates the first date displayed based on the <see cref="SelectedDate"/>, <see cref="Mode"/>, and <see cref="SelectedDatePosition"/>.
    /// </summary>
    /// <returns>In week/workweek modes, the first day of the (work)week.
    /// In Day mode, it will return the first date that positions the <see cref="SelectedDate"/> at the position specified by <see cref="SelectedDatePosition"/>.</returns>
    public DateOnly StartDate()
    {
        if (SelectedDate == DateOnly.MinValue)
        {
            throw new ArgumentException("SelectedDate is not set");
        }

        // Week
        if (Mode == DisplayMode.Week)
        {
            if (SelectedDate.DayOfWeek == FirstDayOfWeek)
            {
                return SelectedDate;
            }

            int x = -(int)SelectedDate.DayOfWeek + (int)FirstDayOfWeek;
            return SelectedDate.AddDays(x);
        }

        // Work Week
        if (Mode == DisplayMode.WorkWeek)
        {
            if (SelectedDate.DayOfWeek == FirstDayOfWeek)
            {
                return SelectedDate;
            }

            var start = SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek + (int)FirstDayOfWeek);
            if (start.DayOfWeek == DayOfWeek.Sunday)
            {
                return start.AddDays(1);
            }

            return start;
        }

        // Day
        if (Days == 1)
        {
            return SelectedDate;
        }

        if (Days > MaxDays)
        {
            throw new ArgumentException($"Days cannot be greater than {MaxDays}");
        }

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
    /// Scrolls the specified item into view.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    public void ScrollIntoView(int index)
    {
        var list = GetItemsAsList();
        if (index < 0 || index >= list.Count)
        {
            return;
        }

        // Get the item in a form we can use.
        var item = Convert(list[index] !, index)?.GetFirstLogicalDescendant<IAppointmentControl>();
        if (item is null)
        {
            return;
        }

        // If it isn't visible, move our visible days so it is visible.
        if (!IsDateVisible(DateOnly.FromDateTime(item.Begin)))
        {
            SelectedDate = DateOnly.FromDateTime(item.Begin);
        }

        InnerScrollIntoView(index);
    }

    /// <summary>
    /// Scrolls the specified item into view.
    /// </summary>
    /// <param name="item">The item.</param>
    public void ScrollIntoView(object item)
    {
        var list = GetItemsAsList();
        var index = list.IndexOf(item);
        if (index < 0 || index >= list.Count)
        {
            return;
        }

        ScrollIntoView(index);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        leftButtonDown = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        base.OnPointerPressed(e);
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!Items.Any() || !leftButtonDown || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            leftButtonDown = false;
            base.OnPointerReleased(e);
            return;
        }

        var control = e.Pointer.Captured as ILogical;
        var appointment = control?.FindLogicalAncestorOfType<IAppointmentControl>();
        var index = appointment?.Index ?? -1;
        leftButtonDown = false;
        base.OnPointerReleased(e);
        var previousIndex = SelectedIndex;
        var currentAutoScrollToSelectedItem = false;
        if (index >= 0)
        {
            currentAutoScrollToSelectedItem = autoScrollToSelectedItem;
            autoScrollToSelectedItem = false;
        }

        SelectedIndex = index;
        if (index >= 0 && currentAutoScrollToSelectedItem)
        {
            autoScrollToSelectedItem = currentAutoScrollToSelectedItem;
        }

        // Force re-trigger
        if (index == previousIndex)
        {
            RaiseSelectionChanged(index);
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up or Key.Left:
                SelectNext(-1);
                e.Handled = true;
                break;
            case Key.Down or Key.Right:
                SelectNext(1);
                e.Handled = true;
                break;
            case Key.Delete:
                DeleteAppointment();
                e.Handled = true;
                break;
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Items changed event.
    /// </summary>
    /// <param name="e">Argument for the event.</param>
    protected void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        collectionChangeSubscription?.Dispose();
        collectionChangeSubscription = null;

        // Add handler for newValue.CollectionChanged (if possible)
        if (e.NewValue is INotifyCollectionChanged newValueINotifyCollectionChanged)
        {
            collectionChangeSubscription = newValueINotifyCollectionChanged.WeakSubscribe(ItemsCollectionChanged!);
        }

        if (skipItemsChanged)
        {
            return;
        }

        ClearItemsGrid();

        if (e.NewValue is not IEnumerable value)
        {
            return;
        }

        UpdateItems(value, SelectedIndex);
    }

    /// <summary>
    /// Gets the items as a list.
    /// </summary>
    /// <returns>An <c>IList</c> representing the appointment <see cref="Items">Items</see>.</returns>
    private IList GetItemsAsList() => Items as IList ?? Items.ToList();

    /// <summary>
    /// Select previous day click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void PreviousDayButtonClick(object? sender, RoutedEventArgs e)
    {
        SelectedDate = SelectedDate.AddDays(-DaysToMove);
    }

    /// <summary>
    /// Select next day click.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void NextDayButtonClick(object? sender, RoutedEventArgs e)
    {
        SelectedDate = SelectedDate.AddDays(DaysToMove);
    }

    /// <summary>
    /// Select next appointment.
    /// </summary>
    /// <param name="step">Step to take.</param>
    private void SelectNext(int step)
    {
        if (itemsGrid == null)
        {
            return;
        }

        var appointments = itemsGrid.GetLogicalDescendants().OfType<IAppointmentControl>().ToList();
        if (appointments.Count == 0)
        {
            return;
        }

        var appointmentIndex = appointments.FindIndex(x => x.Index == SelectedIndex);
        appointmentIndex += step;
        if (appointmentIndex < 0)
        {
            appointmentIndex = appointments.Count - 1;
        }
        else if (appointmentIndex >= appointments.Count)
        {
            appointmentIndex = 0;
        }

        SelectedItem = appointments[appointmentIndex].DataContext;
        if (SelectedItem != null)
        {
            ScrollIntoView(SelectedItem);
        }
    }

    /// <summary>
    /// Deletes the selected appointment.
    /// </summary>
    private void DeleteAppointment()
    {
        if (SelectedIndex < 0 || !AllowDelete)
        {
            return;
        }

        var list = GetItemsAsList();
        try
        {
            list.RemoveAt(SelectedIndex);
            skipItemsChanged = true;
            Items = new ArrayList();
            skipItemsChanged = false;
            Items = list;
        }
        catch (NotSupportedException)
        {
            // Too bad
        }
    }

    /// <summary>
    /// Scroll viewer bounds changed: adjust scrollable grid as well.
    /// </summary>
    /// <param name="rect">Rectangle of the scroll viewer.</param>
    private void OnScrollViewerBoundsChanged(Rect rect) => UpdateScrollViewer(rect, BeginOfTheDay, EndOfTheDay, false);

    /// <summary>
    /// Update the scroll viewers
    /// ///. </summary>
    /// <param name="rect">Rectangle of the scroll viewer.</param>
    /// <param name="beginOfDay">The begin of the day.</param>
    /// <param name="endOfDay">The end of the day.</param>
    /// <param name="forceScroll">Force to scroll or not.</param>
    private void UpdateScrollViewer(Rect rect, TimeSpan beginOfDay, TimeSpan endOfDay, bool forceScroll)
    {
        if (scrollableGrid == null || scrollViewerMain == null)
        {
            return;
        }

        if (rect.Width < 0 || rect.Height < 0)
        {
            return;
        }

        var x = double.NaN;
        var y = double.NaN;
        if (beginOfDay.TotalHours >= 0.0d && endOfDay.TotalHours < 24.0d && endOfDay > beginOfDay)
        {
            var height = 1.0d / (endOfDay - beginOfDay).TotalHours * rect.Height;
            scrollableGrid.Height = height;

            y = forceScroll || scrollViewerMain.Offset.Y.IsZero() ? beginOfDay.TotalHours * height : scrollViewerMain.Offset.Y;
        }
        else
        {
            scrollableGrid.Height = rect.Height;
        }

        scrollableGrid.Width = rect.Width;

        if (double.IsNaN(x) && double.IsNaN(y))
        {
            return;
        }

        x = !double.IsNaN(x) ? x : 0.0d;
        y = !double.IsNaN(y) ? y : 0.0d;

        Dispatcher.UIThread.Post(() => scrollViewerMain.ScrollWithoutBinding(new Vector(x, y)));
    }

    /// <summary>
    /// Checks if the given date is currently visible.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the date is visible, false if not.</returns>
    private bool IsDateVisible(DateOnly date)
    {
        var begin = StartDate();
        var end = begin.AddDays(DaysToShow);
        return date >= begin && date < end;
    }

    /// <summary>
    /// Scrolls the specified item into view.
    /// </summary>
    /// <param name="index">Index of the item.</param>
    private void InnerScrollIntoView(int index)
    {
        if (scrollableGrid == null || scrollViewerMain == null)
        {
            return;
        }

        var control = Convert(GetItemsAsList()[index], index);

        if (control == null)
        {
            return;
        }

        var item = control.GetFirstLogicalDescendant<IAppointmentControl>();

        var scrollViewerMainRect = scrollViewerMain.Bounds;
        var scrollableGridRect = scrollableGrid.Bounds;

        var beginWeek = StartDate().ToDateTime(TimeOnly.MinValue);
        var beginDate = item.Begin.Date;
        var daysOffset = beginDate - beginWeek;
        var begin = (item.Begin - beginDate).TotalDays;

        var x = daysOffset.TotalDays / DaysToShow * scrollableGridRect.Width;
        var y = begin * scrollableGridRect.Height;
        var v = new Vector(x, y);

        if (!GeometryHelper.IsInRect(v, new Rect(scrollViewerMain.Offset.X, scrollViewerMain.Offset.Y, scrollViewerMainRect.Width, scrollViewerMainRect.Height)))
        {
            scrollViewerMain.ScrollWithoutBinding(v);
        }
    }

    /// <summary>
    /// Sets the selection bit for the selected appointment.
    /// </summary>
    /// <param name="selectedIndex">Index of the selected item.</param>
    private void SetSelection(int selectedIndex)
    {
        if (itemsGrid == null)
        {
            return;
        }

        var appointments = itemsGrid.GetLogicalDescendants().OfType<IAppointmentControl>().ToList();
        var appointmentIndex = appointments.FindIndex(x => x.Index == selectedIndex);

        for (var i = 0; i < appointments.Count; i++)
        {
            appointments[i].IsSelected = i == appointmentIndex;
        }
    }

    /// <summary>
    /// Begin of the day changed event.
    /// </summary>
    /// <param name="e">Argument for the event.</param>
    private void BeginOfTheDayChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (scrollViewerMain == null)
        {
            return;
        }

        if (e.NewValue is not TimeSpan value)
        {
            return;
        }

        UpdateScrollViewer(scrollViewerMain.Bounds, value, EndOfTheDay, true);
    }

    /// <summary>
    /// Selected date changed event.
    /// </summary>
    /// <param name="e">Argument for the event.</param>
    private void SelectedDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not DateOnly value)
        {
            return;
        }

        if (value == DateOnly.MinValue)
        {
            return;
        }

        DrawCalendar();
        UpdateItems(Items, SelectedIndex);
    }

    /// <summary>
    /// End of the day changed event.
    /// </summary>
    /// <param name="e">Argument for the event.</param>
    private void EndOfTheDayChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (scrollViewerMain == null)
        {
            return;
        }

        if (e.NewValue is not TimeSpan value)
        {
            return;
        }

        UpdateScrollViewer(scrollViewerMain.Bounds, BeginOfTheDay, value, true);
    }

    /// <summary>
    /// Method that handles the ObservableCollection.CollectionChanged event for the ItemsSource property.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems == null)
        {
            return;
        }

        var newItems = Convert(e.NewItems);
        var visibleItems = newItems.Where(x => x.GetFirstLogicalDescendant<IAppointmentControl>().IsInRange(StartDate().ToDateTime(TimeOnly.MinValue), TimeSpan.FromDays(DaysToShow))).ToList();
        if (visibleItems.Count == 0)
        {
            return;
        }

        UpdateItems(Items, SelectedIndex);
    }

    /// <summary>
    /// Selected index changed event.
    /// </summary>
    /// <param name="e">Argument for the event.</param>
    private void SelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (skipSelectedIndexChanged || e.NewValue is not int value)
        {
            return;
        }

        SetSelection(value);
        AutoScrollToSelectedItemIfNecessary(value);
        RaiseSelectionChanged(value);
    }

    /// <summary>
    /// Selected item changed event.
    /// </summary>
    /// <param name="e">Argument for the event.</param>
    private void SelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (skipSelectedItemChanged || e.NewValue is not { } value)
        {
            return;
        }

        var index = GetItemsAsList().IndexOf(value);
        SetSelection(index);
        AutoScrollToSelectedItemIfNecessary(index);
        RaiseSelectionChanged(value);
    }

    /// <summary>
    /// The mode has changed.
    /// </summary>
    private void ModeChanged()
    {
        DrawCalendar();
        UpdateItems(Items, SelectedIndex);
    }

    /// <summary>
    /// The number of days to display has changed.
    /// </summary>
    /// <param name="e">Event Args.</param>
    private void DaysChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (Mode != DisplayMode.Day)
        {
            return;
        }

        if ((int?)e.NewValue < 1)
        {
            return;
        }

        if ((int?)e.NewValue > MaxDays)
        {
            throw new ArgumentException($"Days must be between 1 and {MaxDays}. Value: {e.NewValue}");
        }

        DrawCalendar();
        UpdateItems(Items, SelectedIndex);
    }

    /// <summary>
    /// Raises the selection changed event.
    /// </summary>
    /// <param name="index">Index selected.</param>
    private void RaiseSelectionChanged(int index)
    {
        object? item = null;
        var list = GetItemsAsList();
        if (index >= 0 && index < list.Count)
        {
            item = list[index];
        }

        var eventArgs = new CalendarSelectionChangedEventArgs(SelectionChangedEvent)
        { SelectedIndex = index, SelectedItem = item };
        RaiseEvent(eventArgs);
        skipSelectedItemChanged = true;
        SelectedItem = item;
        skipSelectedItemChanged = false;
    }

    /// <summary>
    /// Raises the selection changed event.
    /// </summary>
    /// <param name="item">Item selected.</param>
    private void RaiseSelectionChanged(object item)
    {
        var index = GetItemsAsList().IndexOf(item);

        var eventArgs = new CalendarSelectionChangedEventArgs(SelectionChangedEvent)
        { SelectedIndex = index, SelectedItem = item };
        RaiseEvent(eventArgs);
        skipSelectedIndexChanged = true;
        SelectedIndex = index;
        skipSelectedIndexChanged = false;
    }

    /// <summary>
    /// Updated the view with the given items.
    /// </summary>
    /// <param name="enumerable">Items to process.</param>
    /// <param name="selectedIndex">Index of appointment to select.</param>
    private void UpdateItems(IEnumerable enumerable, int selectedIndex)
    {
        if (itemsGrid == null)
        {
            return;
        }

        if (SelectedDate == DateOnly.MinValue)
        {
            return;
        }

        var internalItems = Convert(enumerable);
        var startDate = StartDate().ToDateTime(TimeOnly.MinValue);
        var visibleItems = internalItems.
            Where(x => x.GetFirstLogicalDescendant<IAppointmentControl>().IsInRange(StartDate().ToDateTime(TimeOnly.MinValue), TimeSpan.FromDays(DaysToShow))).
            OrderBy(x => x.GetFirstLogicalDescendant<IAppointmentControl>().Begin).ToList();

        for (var i = 0; i < DaysToShow; i++)
        {
            if (itemsGrid.Children[i] is not Grid dayColumn)
            {
                continue;
            }

            var todayList = visibleItems.Where(x => x.GetFirstLogicalDescendant<IAppointmentControl>().IsInDay(startDate.AddDays(i))).ToList();
            AppointmentControlListHelper.ApplyIndentation(todayList);
            var rowDefinitions = new RowDefinitions();

            var previousEnd = double.NaN;
            var dayControls = new List<Control?>();
            var j = 0;

            while (j < todayList.Count)
            {
                var item = todayList[j].GetFirstLogicalDescendant<IAppointmentControl>();
                var (begin, _) = item.GetFractionOfDay();
                RowDefinitionsHelper.AddEmptyRow(rowDefinitions, dayControls, previousEnd, begin);

                var appointmentGroup = GetAppointmentGroup(todayList, j);
                dayControls.Add(appointmentGroup.Control);
                rowDefinitions.Add(new RowDefinition(appointmentGroup.Length, GridUnitType.Star));
                previousEnd = appointmentGroup.Begin + appointmentGroup.Length;
                j = appointmentGroup.Index;
            }

            // Tail
            RowDefinitionsHelper.AddEmptyRowTail(rowDefinitions, dayControls, previousEnd);
            GridHelper.AddRows(dayColumn, rowDefinitions, dayControls);
        }

        SetSelection(selectedIndex);
    }

    /// <summary>
    /// Get an appointment group from the given list.
    /// </summary>
    /// <param name="list">List to get appointment from.</param>
    /// <param name="beginIndex">Index to begin in.</param>
    /// <returns>
    /// Index to continue in next iteration, the begin (fraction of day), the length (fraction of day) and the
    /// containing control.
    /// </returns>
    private (int Index, double Begin, double Length, Control Control) GetAppointmentGroup(IList<Control> list, int beginIndex)
    {
        var item = list[beginIndex].GetFirstLogicalDescendant<IAppointmentControl>();
        var (begin, _) = item.GetFractionOfDay();
        var count = AppointmentGroupHelper.GetGroupCount(list, beginIndex);
        var end = AppointmentGroupHelper.GetEnd(list, beginIndex, count);
        var length = end - begin;
        var indentationCount = AppointmentGroupHelper.GetIndentationCount(list, beginIndex, count);
        var grid = ControlFactory.CreateGrid(indentationCount);

        grid[!MarginProperty] = new Binding("AppointmentGroupMargin") { Source = this };

        for (var i = 0; i < indentationCount; i++)
        {
            if (grid.Children[i] is not Grid g)
            {
                continue;
            }

            var indentItems = AppointmentGroupHelper.GetIndentationItems(list, beginIndex, count, i);
            if (indentItems.Count == 0)
            {
                continue;
            }

            var groupControls = new List<Control?>();
            var rowDefinitions = new RowDefinitions();
            var previous = double.NaN;
            foreach (var indentItem in indentItems)
            {
                item = indentItem.GetFirstLogicalDescendant<IAppointmentControl>();
                var (b, l) = item.GetFractionOfDay();

                // Within the group
                b = (b - begin) / length;
                l /= length;

                var emptyLength = b - (!double.IsNaN(previous) ? previous : 0.0d);
                if (emptyLength > 0.0d)
                {
                    groupControls.Add(null);
                    rowDefinitions.Add(new RowDefinition(emptyLength, GridUnitType.Star));
                }

                groupControls.Add(indentItem);
                rowDefinitions.Add(new RowDefinition(l, GridUnitType.Star));
                previous = b + l;
            }

            // Tail
            if (double.IsNaN(previous) || previous < 1.0d)
            {
                groupControls.Add(null);
                rowDefinitions.Add(new RowDefinition(1.0d - (!double.IsNaN(previous) ? previous : 0.0d), GridUnitType.Star));
            }

            ControlHelper.AddControlsToRows(g, groupControls, rowDefinitions);
        }

        return (beginIndex + count, begin, length, grid);
    }

    /// <summary>
    /// Converts the given items to a handleable format.
    /// </summary>
    /// <param name="enumerable">Items to process.</param>
    /// <returns>Internal handleable format.</returns>
    private IEnumerable<Control> Convert(IEnumerable enumerable)
    {
        var result = new List<Control>();
        if (!CanBuildItems())
        {
            return result;
        }

        var i = 0;
        foreach (var e in enumerable)
        {
            var p = Convert(e, i);
            if (p != null)
            {
                result.Add(p);
            }

            i++;
        }

        return result;
    }

    /// <summary>
    /// Converts an item from the source (items) to its equivalent control.
    /// </summary>
    /// <param name="o">Object from items.</param>
    /// <param name="index">Index to use for control.</param>
    /// <returns>The control or null otherwise.</returns>
    private Control? Convert(object? o, int index)
    {
        if (o is null || BuildItem(o) is not { } p || !p.HasFirstLogicalDescendant<IAppointmentControl>())
        {
            return null;
        }

        var item = p.GetFirstLogicalDescendant<IAppointmentControl>();
        item.Index = index;
        p.DataContext = o;
        return item.IsValid() ? p : null;
    }

    /// <summary>
    /// Checks if an item can be built.
    /// </summary>
    /// <returns>True if it can and false otherwise.</returns>
    private bool CanBuildItems() => ItemTemplate != null || useDefaultItemsTemplate;

    /// <summary>
    /// Builds the item (control).
    /// </summary>
    /// <param name="param">Parameter to supply to the item template builder.</param>
    /// <returns>The item.</returns>
    private Control BuildItem(object param)
    {
        var result = ItemTemplate?.Build(param);
        return result ?? new AppointmentControl
        {
            [!AppointmentControl.BeginProperty] = new Binding("Begin"),
            [!AppointmentControl.EndProperty] = new Binding("End"),
            [!AppointmentControl.TextProperty] = new Binding("Text"),
            [!AppointmentControl.ColorProperty] = new Binding("Color"),
        };
    }

    /// <summary>
    /// Clears the items grid.
    /// </summary>
    private void ClearItemsGrid()
    {
        if (itemsGrid == null)
        {
            return;
        }

        itemsGrid.Children.Clear();
        var columnDefinitions = new ColumnDefinitions();
        List<Grid> dayColumnsToAdd = [];
        for (int i = 0; i < DaysToShow; i++)
        {
            var dayColumn = ControlFactory.CreateColumn();
            dayColumn.RowDefinitions = [];
            columnDefinitions.Add(new ColumnDefinition(1.0d, GridUnitType.Star));
            dayColumnsToAdd.Add(dayColumn);
            Grid.SetColumn(dayColumn, i);
        }

        itemsGrid.Children.AddRange(dayColumnsToAdd);
        itemsGrid.ColumnDefinitions = columnDefinitions;
    }

    /// <summary>
    /// Draws the calendar grid.
    /// </summary>
    private void DrawCalendar()
    {
        if (weekGrid == null)
        {
            return;
        }

        if (SelectedDate == DateOnly.MinValue)
        {
            return;
        }

        CreateHourTexts();

        CreateDayTexts();

        weekGrid.Children.Clear();

        var columnDefinitions = new ColumnDefinitions();

        List<Control> dayToAdd = [];

        var start = StartDate();

        for (int i = 0; i < DaysToShow; i++)
        {
            var date = start.AddDays(i);
            var dayState = ControlFactory.CreateDayStateControl(date.DayOfWeek);
            var dayColumn = ControlFactory.CreateColumn();

            var rowDefinitions = new RowDefinitions();
            var hourCellToAdd = new List<Border>();

            for (var j = 0; j < HoursPerDay; j++)
            {
                var hourCell = ControlFactory.CreateHourCell();
                hourCellToAdd.Add(hourCell);
                Grid.SetRow(hourCell, j);
                rowDefinitions.Add(new RowDefinition(1.0d, GridUnitType.Star));
            }

            dayColumn.Children.AddRange(hourCellToAdd);
            dayColumn.RowDefinitions = rowDefinitions;

            columnDefinitions.Add(new ColumnDefinition(1.0d, GridUnitType.Star));

            dayToAdd.Add(dayState);
            dayToAdd.Add(dayColumn);

            Grid.SetColumn(dayColumn, i);
            Grid.SetColumn(dayState, i);
        }

        weekGrid.Children.AddRange(dayToAdd);
        weekGrid.ColumnDefinitions = columnDefinitions;
        ClearItemsGrid();
    }

    /// <summary>
    /// Creates the day texts.
    /// </summary>
    private void CreateDayTexts()
    {
        if (dayGrid == null)
        {
            return;
        }

        dayGrid.Children.Clear();

        ColumnDefinitions columnDefinitions = [];

        List<TextBlock> dayTextsToAdd = [];

        DateOnly start = StartDate();

        for (int i = 0; i < DaysToShow; i++)
        {
            DateOnly dateOnly = start.AddDays(i);
            var text = DateTimeHelper.DayOfWeekToString(dateOnly.DayOfWeek) + " " + dateOnly.Day;
            var dayText = new TextBlock { Text = text };
            columnDefinitions.Add(new ColumnDefinition(1.0d, GridUnitType.Star));
            dayTextsToAdd.Add(dayText);
            Grid.SetColumn(dayText, i);
        }

        dayGrid.Children.AddRange(dayTextsToAdd);
        dayGrid.ColumnDefinitions = columnDefinitions;
    }

    /// <summary>
    /// Creates the hour texts.
    /// </summary>
    private void CreateHourTexts()
    {
        if (hourGrid == null)
        {
            return;
        }

        hourGrid.Children.Clear();
        var rowDefinitions = new RowDefinitions();
        List<TextBlock> hourTextsToAdd = [];
        for (var j = 0; j < HoursPerDay; j++)
        {
            var text = new DateTime(1970, 1, 1, j, 0, 0, DateTimeKind.Unspecified).ToShortTimeString();
            var hourText = new TextBlock { Text = text };
            hourTextsToAdd.Add(hourText);
            Grid.SetRow(hourText, j);
            rowDefinitions.Add(new RowDefinition(1.0d, GridUnitType.Star));
        }

        hourGrid.Children.AddRange(hourTextsToAdd);
        hourGrid.RowDefinitions = rowDefinitions;
    }

    /// <summary>
    /// Automatically scroll to the selected item if necessary.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    private void AutoScrollToSelectedItemIfNecessary(int index)
    {
        if (!AutoScrollToSelectedItem)
        {
            return;
        }

        ScrollIntoView(index);
    }
}