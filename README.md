# CalendarControl for Avalonia

This is a calendar control (week view) for Avalonia. See and run the sample app to get to know it.

![CalendarControl screenshot](/Images/CalendarControl.png)

## Note for Avalonia 11.0

Since Avalonia 11.0 there's a breaking change: the namespace changed from SatialInterfaces.Controls to SatialInterfaces.Controls.Calendar. Please update your code.

## How to use

First add the package to your project. Use NuGet to get it: https://www.nuget.org/packages/CalendarControl.Avalonia/

Or use this command in the Package Manager console to install the package manually
```
Install-Package CalendarControl.Avalonia
```

Second add a style to your App.axaml (from the sample app)

````Xml
<Application
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Styles>
        <!-- Overall style goes here: <FluentTheme Mode="Light"/> -->
        <StyleInclude Source="avares://SampleApp/Themes/FluentCalendarControl.axaml" />
    </Application.Styles>
</Application>
````

Or use the default one

````Xml
<Application
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Styles>
        <!-- Overall style goes here: <FluentTheme Mode="Light"/> -->
        <StyleInclude Source="avares://CalendarControl/Themes/Default.axaml" />
    </Application.Styles>
</Application>
````

Then add the control to your Window.axaml (minimum)

````Xml
<Window xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:si="clr-namespace:SatialInterfaces.Controls.Calendar;assembly=CalendarControl">
    <Grid>
        <si:CalendarControl />
    </Grid>
</Window>
````

It's even better to specify the item template with binding to your view model

````Xml
<Window xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:si="clr-namespace:SatialInterfaces.Controls.Calendar;assembly=CalendarControl">
    <Grid>
        <si:CalendarControl>
            <si:CalendarControl.ItemTemplate>
                <DataTemplate>
                    <si:AppointmentControl Begin="{Binding Begin}" End="{Binding End}" Text="{Binding Text}" />
                </DataTemplate>
            </si:CalendarControl.ItemTemplate>
        </si:CalendarControl>
    </Grid>
</Window>
````

### Parameters for CalendarControl
- **SelectedDate** - The date that is the focus of the calendar. (Default: Today)
- **Mode** - The display mode. (Default: Week)
  - *Week* - A standard 7 day week is displayed, with weekends.
  - *Workweek* - A 5 day week is displayed
  - *Day* - A number of days from 1-10 is displayed.
- **Days** - The number of days to display when *Mode* is *Day*. (Default: 1)
- **SelectedDatePosition** - In Day Mode, places the SelectedDate at the given position on the screen. (Default: Left)
  - *Left* - The SelectedDate is the first date displayed (to the left).
  - *Right* - The SelectedDate is the last date displayed (on the right).
  - *Center* - The SelectedDate is placed in the center of displayed days. If the number of days shown is even, the SelectedDate will be right of center
- **FirstDayOfWeek** - The first day of the week. (Default: Sunday)
- **AllowDelete** - True if the user can delete items. (Default: True)
- **SelectedIndex** - The index of the selected item. -1 for no item selected.
- **SelectedItem** - An object representing the selected item. 
- 
