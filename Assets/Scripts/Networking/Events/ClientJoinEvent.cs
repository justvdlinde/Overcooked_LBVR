using Utils.Core.Events;

public class ClientJoinEvent : IEvent
{
    public readonly IClientConnectionData Data;

    public ClientJoinEvent(IClientConnectionData data)
    {
        Data = data;
    }
}
