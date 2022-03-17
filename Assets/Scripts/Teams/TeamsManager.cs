using System;
using System.Collections.Generic;
using Utils.Core.Events;
using Utils.Core.Services;

public class TeamsManager : IService, IDisposable
{
    /// <summary>
    /// The main list of teams with their player-lists. Automatically kept up to date
    /// </summary>
    public Dictionary<Team, List<IPlayer>> PlayersPerTeam { get; private set; }

    public Action<IPlayer, Team> PlayerJoinedTeamEvent;
    public Action<IPlayer, Team> PlayerLeftTeamEvent;

    private readonly PlayersManager playersService;
    private readonly GlobalEventDispatcher globalEventDispatcher;

    public TeamsManager(PlayersManager playersService, GlobalEventDispatcher globalEventDispatcher)
    {
        this.playersService = playersService;
        this.globalEventDispatcher = globalEventDispatcher;
        globalEventDispatcher.Subscribe<ConnectionSuccessEvent>(OnConnectedEvent);

        PlayersPerTeam = new Dictionary<Team, List<IPlayer>>();
        foreach (object team in Enum.GetValues(typeof(Team)))
        {
            PlayersPerTeam.Add((Team)team, new List<IPlayer>());
        }

        InitializeTeams();
    }

    public void ChangeTeam(IPlayer player, Team from, Team to)
    {
        RemoveFromTeam(player, from);
        AddToTeam(player, to);
        globalEventDispatcher.Invoke(new PlayerChangedTeamEvent(player, to));
    }

    private void RemoveFromTeam(IPlayer player, Team team)
    {
        if (PlayersPerTeam[team].Contains(player))
        {
            PlayersPerTeam[team].Remove(player);
            PlayerLeftTeamEvent?.Invoke(player, team);
        }
    }

    private void AddToTeam(IPlayer player, Team team)
    {
        if (!PlayersPerTeam[team].Contains(player))
        {
            PlayersPerTeam[team].Add(player);
            PlayerJoinedTeamEvent?.Invoke(player, team);
        }
    }

    public void InitializeTeams()
    {
        foreach (Team team in PlayersPerTeam.Keys)
        {
            PlayersPerTeam[team].Clear();
        }

        for (int i = 0; i < playersService.AllPlayers.Count; i++)
        {
            IPlayer player = playersService.AllPlayers[i];
            Team playerTeam = player.Team;
            PlayersPerTeam[playerTeam].Add(player);
        }
    }

    public void Dispose()
    {
        globalEventDispatcher.Unsubscribe<ConnectionSuccessEvent>(OnConnectedEvent);
    }

    private void OnConnectedEvent(ConnectionSuccessEvent @event)
    {
        InitializeTeams();
    }
}

