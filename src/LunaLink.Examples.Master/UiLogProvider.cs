using Microsoft.Extensions.Logging;

namespace LunaLink.Examples.Master;

internal sealed class UiLogBuffer
{
    public event Action<string>? MessageAdded;
    public void Add(string message) => MessageAdded?.Invoke(message);
}

internal sealed class UiLogProvider(UiLogBuffer buffer) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new UiLogger(categoryName, buffer);
    public void Dispose() { }

    private sealed class UiLogger(string category, UiLogBuffer buffer) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => level >= LogLevel.Information;

        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(level)) return;
            var source = category.Split('.').Last();
            buffer.Add($"{DateTime.Now:HH:mm:ss} [{level}] {source}: {formatter(state, exception)}");
        }
    }
}
