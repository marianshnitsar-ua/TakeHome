using Domain;
using Domain.Interfaces;

namespace IngestionApi;

public class InMemoryStore : IMeasurementStore
{
    private readonly List<Measurement> _items = [];
    private readonly object _lock = new();
    private readonly int _maxResults;

    public InMemoryStore(IConfiguration config)
    {
        _maxResults = config.GetValue<int>("Storage:MaxQueryResults", 500);
    }

    public Task AddAsync(Measurement m) { lock (_lock) _items.Add(m); return Task.CompletedTask; }

    public Task<IEnumerable<Measurement>> QueryAsync(string? type, DateTimeOffset since)
    {
        IEnumerable<Measurement> q = _items.Where(x => x.Timestamp >= since);

        if (!string.IsNullOrWhiteSpace(type))
            q = q.Where(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(q.TakeLast(_maxResults)); // prevent overfetch
    }
}
