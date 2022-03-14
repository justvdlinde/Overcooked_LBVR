using System;
using System.Collections.Generic;
using System.Linq;

public class TDMScoreboard : IGameModeScoreboard
{
    public Dictionary<Team, TdmTeamScoreData> Data { get; private set; }

    public Action<TDMScoreEntry> OnEntryAdded;
    public Action<TDMScoreEntry> OnEntryRemoved;
    public Action ScoreChangedEvent;

    private readonly TeamsManager teamsManager;

    public TDMScoreboard(TeamsManager teamsManager)
    {
        this.teamsManager = teamsManager;
        Initialize();

        teamsManager.PlayerJoinedTeamEvent += OnPlayerJoinedTeamEvent;
        teamsManager.PlayerLeftTeamEvent += OnplayerLeftTeamEvent;
    }

    public void Dispose()
    {
        teamsManager.PlayerJoinedTeamEvent -= OnPlayerJoinedTeamEvent;
        teamsManager.PlayerLeftTeamEvent -= OnplayerLeftTeamEvent;
    }

    private void Initialize()
    {
        Data = new Dictionary<Team, TdmTeamScoreData>()
        {
            {Team.Team1, new TdmTeamScoreData() },
            {Team.Team2, new TdmTeamScoreData() }
        };

        foreach (var kvp in Data)
        {
            foreach(IPlayer player in teamsManager.PlayersPerTeam[kvp.Key])
            {
                TDMScoreEntry entry = new TDMScoreEntry(player, 0);
                kvp.Value.Players.Add(entry);
                OnEntryAdded?.Invoke(entry);
            }
        }
        UpdateScoreOrder();
    }

    public void Reset()
    {
        foreach (var kvp in Data)
        {
            foreach (TDMScoreEntry score in kvp.Value.Players)
            {
                score.Player.Stats.Reset();
            }
        }
    }

    private void OnPlayerJoinedTeamEvent(IPlayer player, Team team)
    {
        if (!Data.ContainsKey(team))
            return;

        TdmTeamScoreData teamData = Data[team];
        TDMScoreEntry entry = new TDMScoreEntry(player, 0);
        teamData.Players.Add(entry);
        OnEntryAdded?.Invoke(entry);
        UpdateScoreOrder();
    }

    private void OnplayerLeftTeamEvent(IPlayer player, Team team)
    {
        if (!Data.ContainsKey(team))
            return;

        foreach (TDMScoreEntry entry in Data[team].Players)
        {
            if (entry.Player == player)
            {
                Data[team].Players.Remove(entry);
                OnEntryRemoved?.Invoke(entry);
                UpdateScoreOrder();
                return;
            }
        }
    }

    public void UpdateScoreOrder()
    {
        foreach (Team team in Data.Keys)
        {
            TdmTeamScoreData teamData = Data[team];
            teamData.Players = teamData.Players.OrderByDescending(entry => entry.Score).ToList();
        }
        ScoreChangedEvent?.Invoke();
    }

    public int GetScore(Team team)
    {
        return Data[team].Score;
    }

    public void AddScore(Team team, int score)
    {
        Data[team].Score += score;
        ScoreChangedEvent?.Invoke();
    }
}
