using Utils.Core.Events;

public class ClientLeftEvent : IEvent
{
    public readonly IClientDisconnectData Data;

    public ClientLeftEvent(IClientDisconnectData data)
    {
        Data = data;
    }
}
