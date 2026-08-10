namespace LunaLink.Examples.Master;

internal sealed record TagReading(
    Guid Id, string Name, object? Value, string Quality, DateTimeOffset Timestamp, string DataType);

internal sealed class MasterEvents
{
    public event Action<string>? NodeConnected;
    public event Action<TagReading>? TagReceived;

    public void RaiseNodeConnected(string message) => NodeConnected?.Invoke(message);
    public void RaiseTagReceived(TagReading reading) => TagReceived?.Invoke(reading);
}
