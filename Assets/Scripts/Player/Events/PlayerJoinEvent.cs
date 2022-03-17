using Utils.Core.Events;

public class PlayerJoinEvent : IEvent
{
    public readonly IPlayer Player;

    public PlayerJoinEvent(IPlayer player)
    {
        Player = player;
    }
}
