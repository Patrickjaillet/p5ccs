using P5CCS.Core.Configuration;
using Serilog;
using Serilog.Core;

namespace P5CCS.Core.Logging;

public static class LoggingConfigurator
{
    public static Logger CreateLogger(DebugLogSink debugLogSink)
    {
        AppPaths.EnsureDirectoriesExist();

        var logFilePath = Path.Combine(AppPaths.LogsDirectory, "p5ccs-.log");

        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .WriteTo.Sink(new DebugWindowSerilogSink(debugLogSink))
            .CreateLogger();
    }
}
