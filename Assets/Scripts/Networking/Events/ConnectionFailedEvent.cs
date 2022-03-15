using Utils.Core.Events;

public class ConnectionFailedEvent : IEvent
{
    public readonly IDisconnectInfo Info;

    public ConnectionFailedEvent(IDisconnectInfo info)
    {
        Info = info;
    }
}
