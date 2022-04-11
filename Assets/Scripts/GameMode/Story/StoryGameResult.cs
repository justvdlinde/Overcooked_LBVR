public class StoryGameResult : IGameResult
{
    public GameMode GameMode { get; protected set; }
    public IGameModeScoreboard Scores { get; protected set; }

    public StoryGameResult(StoryMode gameMode, StoryModeScoreboard scores)
    {
        GameMode = gameMode;
        Scores = scores;
    }
}
