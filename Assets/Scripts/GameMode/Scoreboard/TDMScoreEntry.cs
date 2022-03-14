using System.Collections.Generic;

public class TdmTeamScoreData
{
    public int Score { get; set; }
    public List<TDMScoreEntry> Players;

    public TdmTeamScoreData()
    {
        Score = 0;
        Players = new List<TDMScoreEntry>();
    }
}

public class TDMScoreEntry
{
    public IPlayer Player { get; protected set; }
    public int Score => Player.Stats.Kills;
    public int Position { get; set; }

    public TDMScoreEntry(IPlayer player, int position)
    {
        Player = player;
        Position = position;
    }
}