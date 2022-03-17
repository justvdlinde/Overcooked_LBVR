using Utils.Core.Events;

public class GameOverEvent : IEvent
{
    public readonly IGameResult GameResult;

    public GameOverEvent(IGameResult gameResult)
    {
        GameResult = gameResult;
    }
}
