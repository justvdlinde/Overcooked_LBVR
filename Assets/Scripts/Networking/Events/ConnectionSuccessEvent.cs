using Photon.Realtime;
using Utils.Core.Events;

public class ConnectionSuccessEvent : IEvent
{
    public readonly Room Room;

    public ConnectionSuccessEvent(Room room)
    {
        Room = room;
    }
}
