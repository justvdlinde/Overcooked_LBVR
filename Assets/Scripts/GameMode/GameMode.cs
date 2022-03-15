using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum MatchPhase
{
    Undefined,
    PreGame, // inbetween rounds
    Countdown,
    Active,
    PostGame
}

public enum MatchEndType
{
    Undefined,
    ObjectiveReached,
    TimeLimitReached,
    StoppedByOperator
}

public abstract class GameMode : IDisposable 
{
    /// <summary>
    /// The name of the gamemode
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Corresponding enum
    /// </summary>
    public abstract GameModeEnum GameModeEnum { get; }

    /// <summary>
    /// The current match phase
    /// </summary>
    public MatchPhase MatchPhase { get; protected set; }

    /// <summary>
    /// Called when <see cref="MatchPhase"/> has changed
    /// </summary>
    public Action<MatchPhase> MatchPhaseChangedEvent { get; set; }

    /// <summary>
    /// Called when the gamemode's settings have been changed
    /// </summary>
    public Action SettingsChangedEvent { get; set; }

    /// <summary>
    /// Called when a team's score has changed
    /// </summary>
    public Action ScoreChangedEvent { get; set; }

    /// <summary>
    /// The score target of the game, once reached the game ends
    /// </summary>
    public int ScoreTarget { get; protected set; }

    /// <summary>
    /// Full duration of this match, in seconds;
    /// </summary>
    public float MatchDuration { get; protected set; }

    /// <summary>
    /// Timestamp in milliseconds when match was started by masterclient
    /// </summary>
    public float MatchStartTimeStamp { get; protected set; }

    /// <summary>
    /// Time the match has been alive, in milliseconds
    /// </summary>
    public float MatchTimeElapsed => gameTimer.ElapsedTime;// (float)PhotonNetwork.Time - MatchStartTimeStamp;

    /// <summary>
    /// How much time is remaining until the game is over, in milliseconds
    /// </summary>
    public float TimeRemaining => gameTimer.TimeRemaining;

	/// <summary>
	/// Gets the game time mapped to a 0 to 1 value
	/// </summary>
	public float GameTimeProgress01 { get { return Mathf.InverseLerp(0f, MatchDuration, TimeRemaining); } }

    /// <summary>
    /// Timestamp that gets set everytime masterclient initializes a new gamemode, 
    /// is useful if you want to know if the gamemode you disconnected/reconnected from is the same one.
    /// </summary>
    public string GameModeInitializedTimeStamp { get; private set; }

    /// <summary>
    /// Manages points of the gamemode
    /// </summary>
    public IGameModeScoreboard Scoreboard { get; protected set; }

    /// <summary>
    /// Game result on game over
    /// </summary>
    public IGameResult GameResult { get; protected set; }

    protected readonly GlobalEventDispatcher globalEventDispatcher;
    protected readonly PhotonNetworkService networkService;

    protected float countdownDuration;
    protected Timer gameTimer;

    public GameMode(GlobalEventDispatcher globalEventDispatcher, INetworkService networkService)
    {
        this.globalEventDispatcher = globalEventDispatcher;
        this.networkService = networkService as PhotonNetworkService;
        gameTimer = new Timer();

        PhotonNetworkService.RoomPropertiesChangedEvent += OnRoomPropertiesChangedEvent;
        PhotonNetworkService.PhotonEventReceivedEvent += OnPhotonEventReceived;
    }

    public virtual void Dispose()
    {
        PhotonNetworkService.RoomPropertiesChangedEvent -= OnRoomPropertiesChangedEvent;
        PhotonNetworkService.PhotonEventReceivedEvent -= OnPhotonEventReceived;
    }

    public virtual void Setup(GameModeSettings settings = null)
    {
        if (settings == null)
            settings = GetDefaultSettings();

        countdownDuration = settings.countdownDuration;

        if (PhotonNetwork.IsMasterClient)
        {
            MatchDuration = settings.matchDuration;
            gameTimer.Set(MatchDuration);
            ScoreTarget = settings.scoreTarget;
            SetupRoomProperties();
        }
        else
        {
            Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            MatchDuration = (float)properties[RoomPropertiesPhoton.MATCH_DURATION];
            gameTimer.Set(MatchDuration);
            ScoreTarget = (int)properties[RoomPropertiesPhoton.OBJECTIVE_TARGET];
            GameModeInitializedTimeStamp = (string)properties[RoomPropertiesPhoton.GAME_TIME_STAMP];


            if (MatchPhase != MatchPhase.PreGame && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.MATCH_START_TIME, out object value))
                MatchStartTimeStamp = (float)value;

            MatchPhase phase = (MatchPhase)(int)properties[RoomPropertiesPhoton.GAME_STATE];
            if (MatchPhase != phase)
                InvokePhase(phase);
        }
    }

    public void OnReconnect()
    {
        Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        MatchDuration = (float)properties[RoomPropertiesPhoton.MATCH_DURATION];
        ScoreTarget = (int)properties[RoomPropertiesPhoton.OBJECTIVE_TARGET];
        GameModeInitializedTimeStamp = (string)properties[RoomPropertiesPhoton.GAME_TIME_STAMP];

        MatchPhase phase = (MatchPhase)(int)properties[RoomPropertiesPhoton.GAME_STATE];
        if (MatchPhase != phase)
            InvokePhase(phase);
    }

    /// <summary>
    /// Can the game start?
    /// </summary>
    /// <returns></returns>
    public abstract bool StartRequirementsAreMet();

    protected abstract GameModeSettings GetDefaultSettings();

    protected virtual void SetupRoomProperties()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        GameModeInitializedTimeStamp = DateTime.Now.ToString();
        Dictionary<string, object> properties = new Dictionary<string, object>
        {
            { RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase },
            { RoomPropertiesPhoton.MATCH_DURATION, MatchDuration },
            { RoomPropertiesPhoton.OBJECTIVE_TARGET, ScoreTarget },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        SettingsChangedEvent?.Invoke();
    }

    protected virtual void OnRoomPropertiesChangedEvent(Hashtable properties)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        if (properties.TryGetValue(RoomPropertiesPhoton.MATCH_DURATION, out object newValue))
        {
            MatchDuration = (float)newValue;
            gameTimer.Set(MatchDuration);
        }
        if (properties.TryGetValue(RoomPropertiesPhoton.OBJECTIVE_TARGET, out newValue))
            ScoreTarget = (int)newValue;
        if (properties.TryGetValue(RoomPropertiesPhoton.MATCH_START_TIME, out newValue))
            MatchStartTimeStamp = (float)newValue;
        if (properties.TryGetValue(RoomPropertiesPhoton.GAME_TIME_STAMP, out newValue))
            GameModeInitializedTimeStamp = (string)newValue;

        SettingsChangedEvent?.Invoke();
    }

    protected void InvokePhase(MatchPhase phase)
    {
        switch (phase)
        {
            case MatchPhase.PreGame:
                PreGame(false);
                break;
            case MatchPhase.Active:
                StartActiveGame();
                break;
            case MatchPhase.PostGame:
                EndGame();
                break;
            case MatchPhase.Undefined:
                Shutdown();
                break;
        }
    }

    protected void SetPhase(MatchPhase newPhase)
    {
        MatchPhase = newPhase;
        MatchPhaseChangedEvent?.Invoke(newPhase);
		globalEventDispatcher.Invoke(new GameModePhaseChangedEvent(newPhase));
    }

    public virtual void PreGame(bool replay = false)
    {
        SetPhase(MatchPhase.PreGame);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);
            if (replay)
                GameModeInitializedTimeStamp = DateTime.Now.ToString();
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_TIME_STAMP, GameModeInitializedTimeStamp.ToString());
        }

        if (replay)
        {
            Scoreboard.Reset();
            gameTimer.Reset();
        }
    }

    public virtual void StartCountdown()
    {
        SetPhase(MatchPhase.Countdown);
		if (PhotonNetwork.IsMasterClient)
        {
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);
            RaisePhotonEventCode(PhotonEventCodes.GAME_START);
        }
        Timer countdownTimer = new Timer();
        countdownTimer.Set(countdownDuration);
        countdownTimer.Start(StartActiveGame);
    }

    public virtual void StartActiveGame()
    {
        SetPhase(MatchPhase.Active);
        globalEventDispatcher.Invoke(new StartGameEvent());

        if (PhotonNetwork.IsMasterClient)
        {
			MatchStartTimeStamp = (float)PhotonNetwork.Time;
            Dictionary<string, object> properties = new Dictionary<string, object>
            {
                { RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase },
                { RoomPropertiesPhoton.MATCH_START_TIME, MatchStartTimeStamp }
            };

            RaisePhotonEventCode(PhotonEventCodes.GAME_ACTIVE);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

        // In case a player joins mid-game, the start time and current network time have to be subtracted from the match duration 
        float duration = MatchDuration; 
        if(MatchStartTimeStamp != 0)
            duration = Mathf.Clamp(MatchDuration - ((float)PhotonNetwork.Time - MatchStartTimeStamp), 0, MatchDuration);

        gameTimer.Set(duration);
        gameTimer.Start(OnTimerReachedZero);
    }

    protected virtual void OnTimerReachedZero()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameTimer.Stop();
            EndGame(MatchEndType.TimeLimitReached);
        }
    }

    public virtual void EndGame(MatchEndType endType = MatchEndType.Undefined)
    {
        SetPhase(MatchPhase.PostGame);
        gameTimer.Stop();

        if (PhotonNetwork.IsMasterClient)
        {
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);
            object[] eventContent = new object[] { (int)endType };
            RaisePhotonEventCode(PhotonEventCodes.GAME_STOP, eventContent);
        }
    }

    public virtual void Shutdown()
    {
        SetPhase(MatchPhase.Undefined);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);

        Scoreboard.Dispose();
    }

    public virtual void Replay()
    {
        if (PhotonNetwork.IsMasterClient)
            RaisePhotonEventCode(PhotonEventCodes.GAME_RESTART);

        if (MatchPhase == MatchPhase.Active)
            EndGame();

        Shutdown();
        PreGame(true);
        Scoreboard.Reset();
        globalEventDispatcher.Invoke(new ReplayEvent());
    }

    /// <summary>
    /// Returns TimeRemaining as a readable time in minutes and seconds
    /// </summary>
    /// <returns></returns>
    public virtual string GetTimeReadableString()
    {
        int minutes = Mathf.FloorToInt(TimeRemaining / 60f);
        int seconds = Mathf.FloorToInt(TimeRemaining - minutes * 60);
        return string.Format("{00:00}:{1:00}", minutes, seconds);
    }

    public virtual void SetMatchDuration(float newDuration)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.MATCH_DURATION, newDuration);
        MatchDuration = newDuration;
        SettingsChangedEvent?.Invoke();
    }

    public virtual void SetScoreTarget(int newScore)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.OBJECTIVE_TARGET, newScore);
        ScoreTarget = newScore;
        SettingsChangedEvent?.Invoke();
    }

    /// <summary>
    /// Raises a Photon event code for specific game logic such as starting and stopping
    /// </summary>
    /// <param name="code">see<see cref="PhotonEventCodes"/></param>
    /// <param name="receivers"></param>
    private void RaisePhotonEventCode(int code, object[] content = null, ReceiverGroup receivers = ReceiverGroup.Others)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = receivers };
        PhotonNetwork.RaiseEvent((byte)code, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnPhotonEventReceived(EventData data)
    {
        switch (data.Code)
        {
            case PhotonEventCodes.GAME_START:
                StartCountdown();
                break;
            case PhotonEventCodes.GAME_ACTIVE:
                StartActiveGame();
                break;
            case PhotonEventCodes.GAME_STOP:
                object[] content = (object[])data.CustomData;
                EndGame((MatchEndType)(int)content[0]);
                break;
            case PhotonEventCodes.GAME_RESTART:
                Replay();
                break;
        }
    }
}


