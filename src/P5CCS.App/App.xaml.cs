using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using P5CCS.App.ViewModels;
using P5CCS.Core.Configuration;
using P5CCS.Core.Logging;
using P5CCS.Core.Preferences;
using P5CCS.Core.Projects;
using P5CCS.Core.Versioning;
using Serilog;

namespace P5CCS.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private Serilog.Core.Logger? _serilogLogger;

    protected override void OnStartup(StartupEventArgs e)
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

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Application starting up");

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serilogLogger?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
