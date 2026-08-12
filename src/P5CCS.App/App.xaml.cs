using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using P5CCS.App.Services;
using P5CCS.App.ViewModels;
using P5CCS.Core.Configuration;
using P5CCS.Core.Dialogs;
using P5CCS.Core.Input;
using P5CCS.Core.Logging;
using P5CCS.Core.Preferences;
using P5CCS.Core.Projects;
using P5CCS.Core.Theming;
using P5CCS.Core.Versioning;
using Serilog;

namespace P5CCS.App;

public partial class App : Application
{
    private const int MinimumSplashDurationMilliseconds = 800;

    private ServiceProvider? _serviceProvider;
    private Serilog.Core.Logger? _serilogLogger;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var debugLogSink = new DebugLogSink();
        _serilogLogger = LoggingConfigurator.CreateLogger(debugLogSink);

        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddSerilog(_serilogLogger, dispose: false));

        services.AddSingleton<IDebugLogSink>(debugLogSink);
        services.AddSingleton(debugLogSink);
        services.AddSingleton<IVersionService, VersionService>();
        services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
        services.AddSingleton<IPreferencesService, PreferencesService>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IKeyBindingsService, KeyBindingsService>();
        services.AddSingleton<IThemeService, WpfThemeService>();
        services.AddSingleton<IDialogService, WpfDialogService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Application starting up");

        var versionService = _serviceProvider.GetRequiredService<IVersionService>();
        var splash = new SplashWindow(versionService.InformationalVersion);
        splash.Show();

        var splashStopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Dispatcher.Yield(DispatcherPriority.Render);

        var themeService = _serviceProvider.GetRequiredService<IThemeService>();
        var preferencesService = _serviceProvider.GetRequiredService<IPreferencesService>();
        themeService.ApplyTheme(preferencesService.Current.Theme);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        var remainingDelay = MinimumSplashDurationMilliseconds - (int)splashStopwatch.ElapsedMilliseconds;
        if (remainingDelay > 0)
        {
            await Task.Delay(remainingDelay);
        }

        mainWindow.Show();
        splash.Close();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serilogLogger?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
