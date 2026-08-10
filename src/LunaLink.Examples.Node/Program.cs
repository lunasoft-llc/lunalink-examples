using LunaLink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LunaLink.Examples.Node;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var builder = Host.CreateApplicationBuilder();
        var logs = new UiLogBuffer();
        builder.Logging.AddProvider(new UiLogProvider(logs));
        builder.Services.AddSingleton(logs);
        builder.Services.Configure<LunaLinkOptions>(
            builder.Configuration.GetSection(LunaLinkOptions.Section));
        builder.Services.AddSingleton<NodeState>();
        builder.Services.AddSingleton<ILunaLinkClientCallback, NodeCallback>();
        builder.Services.AddSingleton<LunaLinkOutbox>();
        builder.Services.AddSingleton<NodeClientController>();
        builder.Services.AddSingleton<MainForm>();

        using var host = builder.Build();
        host.Start();
        Application.Run(host.Services.GetRequiredService<MainForm>());
        host.Services.GetRequiredService<NodeClientController>().DisconnectAsync().GetAwaiter().GetResult();
        host.StopAsync().GetAwaiter().GetResult();
    }
}
