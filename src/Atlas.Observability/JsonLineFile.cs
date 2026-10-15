using System.Text;

namespace Atlas.Observability;

/// <summary>Size-bounded JSONL files. Separate prefixes keep diagnostics, metrics, traces and audit readable.</summary>
public sealed class JsonLineFile
{
    private readonly object _gate = new();
    private readonly string _directory;
    private readonly string _prefix;
    private readonly long _maxBytes;
    private readonly int _retainedFiles;
    private readonly string _instance = Guid.NewGuid().ToString("N");
    private int _sequence;

    public JsonLineFile(string directory, string prefix, long maxBytes = 5 * 1024 * 1024, int retainedFiles = 5)
    {
        if (maxBytes < 1024 || retainedFiles < 1) throw new ArgumentOutOfRangeException(nameof(maxBytes));
        if (prefix is not ("diagnostics" or "audit" or "metrics" or "traces")) throw new ArgumentException("Unknown stream.");
        _directory = Path.GetFullPath(directory);
        _prefix = prefix;
        _maxBytes = maxBytes;
        _retainedFiles = retainedFiles;
        Directory.CreateDirectory(_directory);
    }

    /// <summary>Return only after flushing the local write. A crash before SQL acknowledgement can duplicate an audit line.</summary>
    public void Write(string json)
    {
        // A serialized record must be one physical line; JSON encoding escapes payload newlines.
        if (json.Contains('\n') || json.Contains('\r')) throw new ArgumentException("Expected one JSON line.");
        var bytes = Encoding.UTF8.GetBytes(json + Environment.NewLine);
        lock (_gate)
        {
            var path = FilePath();
            if (File.Exists(path) && new FileInfo(path).Length + bytes.Length > _maxBytes)
            {
                _sequence++;
                path = FilePath();
            }
            using (var file = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                file.Write(bytes);
                file.Flush(flushToDisk: true);
            }
            // Audit history is retained in SQL; these are local delivered copies. Pending SQL
            // records never participate in file rotation. All rotated copies share one retention budget across restarts.
            foreach (var old in Directory.GetFiles(_directory, $"{_prefix}-*.jsonl")
                .OrderByDescending(File.GetLastWriteTimeUtc).ThenByDescending(value => value, StringComparer.Ordinal).Skip(_retainedFiles))
                File.Delete(old);
        }
    }

    private string FilePath() => Path.Combine(_directory, $"{_prefix}-{_instance}-{_sequence:D8}.jsonl");
}
