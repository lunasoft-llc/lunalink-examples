using LunaLink;

namespace LunaLink.Examples.Node;

internal sealed class NodeCallback(NodeState state) : ILunaLinkClientCallback
{
    public Task<List<LunaLinkTagSnapshot>> GetSnapshotAsync(IEnumerable<Guid>? tagIds, CancellationToken ct)
    {
        var filter = tagIds?.ToHashSet();
        var values = state.Values.Where(x => filter is null || filter.Contains(x.TagId)).ToList();
        return Task.FromResult(values);
    }

    public Task ProcessDataPointAsync(Guid tagId, string? tagName, object? value,
        LunaLinkQuality quality, DateTimeOffset timestamp, LunaLinkDataType dataType) => Task.CompletedTask;

    public Task<bool> WriteTagAsync(Guid tagId, string? tagName, object? value, CancellationToken ct)
    {
        if (!state.TryGet(tagId, out var current) || current is null) return Task.FromResult(false);
        current.Value = value;
        current.Timestamp = DateTimeOffset.UtcNow;
        state.Set(current);
        return Task.FromResult(true);
    }

    public Task<int> GetConnectedDeviceCountAsync(CancellationToken ct) => Task.FromResult(1);
}
