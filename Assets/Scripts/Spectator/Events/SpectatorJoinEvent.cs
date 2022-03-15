using Utils.Core.Events;

public class SpectatorJoinEvent : IEvent
{
    public readonly Spectator spectator;

    public SpectatorJoinEvent(Spectator spectator)
    {
        this.spectator = spectator;
    }
}
