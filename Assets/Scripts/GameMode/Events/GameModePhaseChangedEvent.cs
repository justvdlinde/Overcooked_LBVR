using Utils.Core.Events;

public class GameModePhaseChangedEvent : IEvent
{
    public readonly MatchPhase Phase;

    public GameModePhaseChangedEvent(MatchPhase phase)
    {
        Phase = phase;
    }
}
