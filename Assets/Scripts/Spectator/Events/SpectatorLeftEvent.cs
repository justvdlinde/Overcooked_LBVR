using Utils.Core.Events;

public class SpectatorLeftEvent : IEvent
{
    public readonly Spectator spectator;

    public SpectatorLeftEvent(Spectator spectator)
    {
        this.spectator = spectator;
    }
}
