using System.Collections.Concurrent;
using LunaLink;

namespace LunaLink.Examples.Node;

internal sealed class NodeState
{
    public static readonly Guid TemperatureId = Guid.Parse("a82dfaa9-b4aa-46ad-9b41-91a92f976001");
    public static readonly Guid PressureId = Guid.Parse("a82dfaa9-b4aa-46ad-9b41-91a92f976002");

    private readonly ConcurrentDictionary<Guid, LunaLinkTagSnapshot> _values = new();

    public IReadOnlyCollection<LunaLinkTagSnapshot> Values => _values.Values.ToArray();
    public void Set(LunaLinkTagSnapshot value) => _values[value.TagId] = value;
    public bool TryGet(Guid id, out LunaLinkTagSnapshot? value) => _values.TryGetValue(id, out value);
}
