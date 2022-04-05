public class StoryGameResult : IGameResult
{
    public GameMode GameMode { get; protected set; }
    public IGameModeScoreboard Scores { get; protected set; }

    public StoryGameResult(StoryGameMode gameMode, StoryGameScores scores)
    {
        GameMode = gameMode;
        Scores = scores;
    }
}
