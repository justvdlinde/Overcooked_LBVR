using Utils.Core.Events;

public class GameModeChangedEvent : IEvent
{
    public readonly GameMode GameMode;

    public GameModeChangedEvent(GameMode gameMode)
    {
        GameMode = gameMode;
    }
}
