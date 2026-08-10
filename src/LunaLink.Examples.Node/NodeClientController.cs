using LunaLink;
using Microsoft.Extensions.DependencyInjection;

namespace LunaLink.Examples.Node;

internal sealed class NodeClientController(IServiceProvider services) : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private LunaLinkClient? _client;

    public bool IsRunning => _client is not null;
    public bool IsConnected => _client?.IsConnected ?? false;

    public async Task ConnectAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_client is not null) return;
            // LunaLink 1.0.48 requires hosts to initialize the shared outbox.
            // Newer package versions also perform this safely during client startup.
            await services.GetRequiredService<LunaLinkOutbox>().InitializeAsync();
            var client = ActivatorUtilities.CreateInstance<LunaLinkClient>(services);
            await client.StartAsync(CancellationToken.None);
            _client = client;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_client is null) return;
            await _client.StopAsync(CancellationToken.None);
            _client.Dispose();
            _client = null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task SendAsync(
        IEnumerable<(Guid tagId, string? tagName, object? value, LunaLinkQuality quality,
            DateTimeOffset timestamp, LunaLinkDataType dataType)> deltas,
        CancellationToken ct = default) =>
        _client?.SendTagDeltaAsync(deltas, ct)
        ?? throw new InvalidOperationException("Connect the LunaLink client first.");

    public void Dispose()
    {
        _client?.Dispose();
        _gate.Dispose();
    }
}
