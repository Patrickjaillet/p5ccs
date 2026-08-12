using System.Collections.ObjectModel;

namespace P5CCS.Core.Logging;

public interface IDebugLogSink
{
    ReadOnlyObservableCollection<LogEntry> Entries { get; }

    void Clear();
}
