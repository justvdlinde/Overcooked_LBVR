using Utils.Core.Events;

public class PlayerLeftEvent : IEvent
{
    public readonly IPlayer Player;

    public PlayerLeftEvent(IPlayer player)
    {
        Player = player;
    }
}
