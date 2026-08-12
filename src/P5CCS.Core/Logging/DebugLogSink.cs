using System.Collections.ObjectModel;

namespace P5CCS.Core.Logging;

public sealed class DebugLogSink : IDebugLogSink
{
    private const int MaxEntries = 2000;

    private readonly ObservableCollection<LogEntry> _entries = new();
    private readonly object _syncRoot = new();

    public DebugLogSink()
    {
        Entries = new ReadOnlyObservableCollection<LogEntry>(_entries);
    }

    public ReadOnlyObservableCollection<LogEntry> Entries { get; }

    public void Add(LogEntry entry)
    {
        lock (_syncRoot)
        {
            _entries.Add(entry);
            while (_entries.Count > MaxEntries)
            {
                _entries.RemoveAt(0);
            }
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _entries.Clear();
        }
    }
}
