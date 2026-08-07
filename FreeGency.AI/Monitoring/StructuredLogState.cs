using System.Collections;

namespace FreeGency.AI.Monitoring;

/// <summary>
/// A lightweight, immutable log state implementing the structured-logging shape
/// consumed by <see cref="Microsoft.Extensions.Logging.ILogger"/>. Every log line
/// produced by <see cref="ModerationStructuredLogger"/> is backed by this state so
/// all key-value fields (event name, request id, correlation id, UTC timestamp,
/// ...) are captured by any structured log sink.
/// </summary>
internal sealed class StructuredLogState : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly KeyValuePair<string, object?>[] _fields;
    private readonly string _message;

    public StructuredLogState(string message, IEnumerable<KeyValuePair<string, object?>> fields)
    {
        _message = message;
        _fields = fields.ToArray();
    }

    public KeyValuePair<string, object?> this[int index] => _fields[index];

    public int Count => _fields.Length;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        => ((IEnumerable<KeyValuePair<string, object?>>)_fields).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => _message;
}
