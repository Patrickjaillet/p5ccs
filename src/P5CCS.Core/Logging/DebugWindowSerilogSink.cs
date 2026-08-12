using Serilog.Core;
using Serilog.Events;

namespace P5CCS.Core.Logging;

public sealed class DebugWindowSerilogSink : ILogEventSink
{
    private readonly DebugLogSink _target;

    public DebugWindowSerilogSink(DebugLogSink target)
    {
        _target = target;
    }

    public void Emit(LogEvent logEvent)
    {
        var entry = new LogEntry(
            logEvent.Timestamp,
            logEvent.Level.ToString(),
            logEvent.RenderMessage(),
            logEvent.Exception?.ToString());

        _target.Add(entry);
    }
}
