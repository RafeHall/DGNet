using DGNet.Serde;

namespace DGNet;

public readonly record struct EventGroupTableEntry(
    string Name,
    EventGroupTableEntry.SerializeDelegate Serialize,
    EventGroupTableEntry.DeserializeDelegate Deserialize
    )
{
    public delegate void SerializeDelegate(byte @event, object self, Serializer se);
    public delegate object DeserializeDelegate(byte @event, Deserializer de);
}

public class Tables
{
    private readonly EventGroupTableEntry[] _eventGroups;

    public Tables()
    {
        _eventGroups = [
            // new(
            //     "VoteEvent",
            //     VoteEvent.Serialize,
            //     VoteEvent.Deserialize
            // )
        ];
    }

    private void HandleEventGroupPacket(ExampleEventGroupPacket packet)
    {
        var id = packet.Id;
        var eventGroup = _eventGroups[id];

    }

    private struct ExampleEventGroupPacket
    {
        public uint Id;
        public byte[] Data;
    }
}
