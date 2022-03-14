using Photon.Pun;
using UnityEngine;
using Utils.Core.Events;

public class TDMGameMode : GameMode
{
    public override string Name => "Team Deathmatch";
    public new TDMScoreboard Scoreboard => base.Scoreboard as TDMScoreboard;

    public override GameModeEnum GameModeEnum => GameModeEnum.TeamDeathmatch;

    private readonly TeamsManager teamsManager;

    public TDMGameMode(GlobalEventDispatcher globalEventDispatcher, INetworkService networkService, TeamsManager teamsManager) : base(globalEventDispatcher, networkService) 
    {
        this.teamsManager = teamsManager;
    }

    protected override GameModeSettings GetDefaultSettings()
    {
        return ScriptableObject.CreateInstance<GameModeSettings>();
    }

    public override void Setup(GameModeSettings settings)
    {
        base.Scoreboard = new TDMScoreboard(teamsManager);

        if (settings == null)
            settings = GetDefaultSettings();

        // Instantiate a new settings object so we don't overwrite the original config in case there are existing prefs we want to use
        settings = Object.Instantiate(settings);
        ////Set settings to the stored prefs, in case these are found
        //bool prefFound = false;
        //if (PlayerPrefs.HasKey(OperatorPlayerPrefs.PREF_TDM_GAME_GOAL))
        //{
        //    settings.scoreTarget = PlayerPrefs.GetInt(OperatorPlayerPrefs.PREF_TDM_GAME_GOAL);
        //    prefFound = true;
        //}
        //if (PlayerPrefs.HasKey(OperatorPlayerPrefs.PREF_TDM_GAME_DURATION))
        //{
        //    settings.matchDuration = PlayerPrefs.GetFloat(OperatorPlayerPrefs.PREF_TDM_GAME_DURATION);
        //    Debug.Log("setting match duration: " + MatchDuration);
        //    prefFound = true;
        //}

        base.Setup(settings);
    }

    /// <summary>
    /// TDM can only start if both teams have at least one player 
    /// </summary>
    /// <returns></returns>
    public override bool StartRequirementsAreMet()
    {
        return teamsManager.PlayersPerTeam[Team.Team1].Count > 0 && teamsManager.PlayersPerTeam[Team.Team2].Count > 0;
    }

    public override void StartActiveGame()
    {
        base.StartActiveGame();
        globalEventDispatcher.Subscribe<PlayerDeathEvent>(OnPlayerDeathEvent);
    }

    public override void EndGame(MatchEndType endType)
    {
        Team winner = DecideWinner();
        GameResult = new TDMGameResult(this, endType, winner);
        base.EndGame(endType);

        globalEventDispatcher.Invoke(new GameOverEvent(GameResult));
        globalEventDispatcher.Unsubscribe<PlayerDeathEvent>(OnPlayerDeathEvent);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        globalEventDispatcher.Unsubscribe<PlayerDeathEvent>(OnPlayerDeathEvent);
    }

    private void OnPlayerDeathEvent(PlayerDeathEvent @event)
    {
        IPlayer deadPlayer;
        if (@event.Info.DeadPlayer == null)
            return;

        deadPlayer = @event.Info.DeadPlayer;
        deadPlayer.Stats.Deaths.Add(1);

        PlayerKilledInfo info = @event.Info as PlayerKilledInfo;
        if (info == null || info.Killer == null)
            return;

        IPlayer killer = info.Killer;

        // Check for friendly fire
        if (killer.Team != deadPlayer.Team)
        {
            Debug.LogFormat("{0} killed {1}", (killer != null) ? killer.Name : "Null", deadPlayer.Name);

            if (PhotonNetwork.IsMasterClient)
            {
                killer.Stats.Kills.Add(1);
                Scoreboard.AddScore(killer.Team, 1);
                ScoreChangedEvent?.Invoke();

                if (IsKillTargetReached(killer.Team))
                    OnKillTargetReached();
            }
        }
    }

    private bool IsKillTargetReached(Team team)
    {
        return Scoreboard.GetScore(team) >= ScoreTarget;
    }

    private void OnKillTargetReached()
    {
        Debug.LogFormat("Team kill target is reached team1: {0} team2: {1}", Scoreboard.GetScore(Team.Team1), Scoreboard.GetScore(Team.Team2));
        if (PhotonNetwork.IsMasterClient)
            EndGame(MatchEndType.ObjectiveReached);
    }

    private Team DecideWinner()
    {
        int team1Score = Scoreboard.GetScore(Team.Team1);
        int team2Score = Scoreboard.GetScore(Team.Team2);

        if (team1Score == team2Score)
            return Team.None;

        Team bestTeam = (team1Score > team2Score) ? Team.Team1 : Team.Team2;
        return bestTeam;
    }

    public override void SetScoreTarget(int newScore)
    {
        base.SetScoreTarget(newScore);
        // if standalone:
        // PlayerPrefs.SetInt(OperatorPlayerPrefs.PREF_TDM_GAME_GOAL, newScore);
    }

    public override void SetMatchDuration(float newDuration)
    {
        base.SetMatchDuration(newDuration);
        // if standalone:
        // PlayerPrefs.SetFloat(OperatorPlayerPrefs.PREF_TDM_GAME_DURATION, newDuration);
    }

    public override void Dispose()
    {
        base.Dispose();
        globalEventDispatcher.Unsubscribe<PlayerDeathEvent>(OnPlayerDeathEvent);
    }
}
