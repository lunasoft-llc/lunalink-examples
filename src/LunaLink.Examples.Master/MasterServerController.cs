using LunaLink;
using Microsoft.Extensions.DependencyInjection;

namespace LunaLink.Examples.Master;

internal sealed class MasterServerController(IServiceProvider services) : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private LunaLinkServer? _server;

    public bool IsListening => _server is not null;

    public async Task StartAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_server is not null) return;
            var server = ActivatorUtilities.CreateInstance<LunaLinkServer>(services);
            await server.StartAsync(CancellationToken.None);
            _server = server;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task StopAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_server is null) return;
            await _server.StopAsync(CancellationToken.None);
            _server.Dispose();
            _server = null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _server?.Dispose();
        _gate.Dispose();
    }
}
