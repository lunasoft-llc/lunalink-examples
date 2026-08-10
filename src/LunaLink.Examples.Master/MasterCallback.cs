using LunaLink;
using Microsoft.Extensions.Logging;

namespace LunaLink.Examples.Master;

internal sealed class MasterCallback(MasterEvents events, ILogger<MasterCallback> logger) : ILunaLinkServerCallback
{
    public Task OnNodeHelloAsync(string nodeId, string nodeName, string remoteEndpoint, CancellationToken ct)
    {
        events.RaiseNodeConnected($"{nodeName} ({nodeId}) — {remoteEndpoint}");
        logger.LogInformation("Node connected: {NodeName} ({NodeId}) from {RemoteEndpoint}", nodeName, nodeId, remoteEndpoint);
        return Task.CompletedTask;
    }

    public Task ProcessDataPointAsync(Guid tagId, string? tagName, object? value,
        LunaLinkQuality quality, DateTimeOffset timestamp, LunaLinkDataType dataType)
    {
        events.RaiseTagReceived(new TagReading(
            tagId, tagName ?? tagId.ToString(), value, quality.ToString(), timestamp, dataType.ToString()));
        logger.LogInformation("Tag {TagName} = {Value} ({Quality})", tagName ?? tagId.ToString(), value, quality);
        return Task.CompletedTask;
    }

    public Task<List<LunaLinkTagSnapshot>> GetSnapshotAsync(IEnumerable<Guid>? tagIds, CancellationToken ct) =>
        Task.FromResult(new List<LunaLinkTagSnapshot>());
}
