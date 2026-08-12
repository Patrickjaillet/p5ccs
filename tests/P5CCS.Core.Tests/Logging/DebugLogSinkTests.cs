using P5CCS.Core.Logging;
using Serilog;

namespace P5CCS.Core.Tests.Logging;

public class DebugLogSinkTests
{
    [Fact]
    public void Emit_ThroughSerilogSink_AddsEntry()
    {
        var sink = new DebugLogSink();
        using var logger = new LoggerConfiguration()
            .WriteTo.Sink(new DebugWindowSerilogSink(sink))
            .CreateLogger();

        logger.Information("Sketch {Name} loaded", "demo.js");

        Assert.Single(sink.Entries);
        Assert.Contains("demo.js", sink.Entries[0].Message);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var sink = new DebugLogSink();
        using var logger = new LoggerConfiguration()
            .WriteTo.Sink(new DebugWindowSerilogSink(sink))
            .CreateLogger();

        logger.Information("First");
        logger.Information("Second");
        sink.Clear();

        Assert.Empty(sink.Entries);
    }
}
