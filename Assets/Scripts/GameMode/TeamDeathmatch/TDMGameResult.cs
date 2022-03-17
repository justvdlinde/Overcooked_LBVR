public class TDMGameResult : IGameResult
{
    public GameMode GameMode { get; protected set; }
    public MatchEndType EndType { get; protected set; }

    public readonly bool IsDraw;
    public readonly Team Winner;

    public TDMGameResult(GameMode gameMode, MatchEndType endType, Team winner)
    {
        GameMode = gameMode;
        EndType = endType;

        Winner = winner;
        IsDraw = winner == Team.None;
    }
}
