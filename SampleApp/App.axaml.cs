using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;

namespace SampleApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        // var theme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
        // if (theme != null)
        //     theme.RequestedTheme = "Dark";

        base.OnFrameworkInitializationCompleted();
    }
}