public interface IGameResult
{
    public GameMode GameMode { get; }
    public IGameModeScoreboard Scores { get; }
}
