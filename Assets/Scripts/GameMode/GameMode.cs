using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum MatchPhase
{
    Undefined,
    PreGame,
    Active,
    PostGame
}

public abstract class GameMode : MonoBehaviourPun, IPunInstantiateMagicCallback
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
    /// Called when a team's score has changed
    /// </summary>
    public Action ScoreChangedEvent { get; set; }

    /// <summary>
    /// Full duration of this match, in seconds;
    /// </summary>
    public float MatchDuration { get; protected set; }

    /// <summary>
    /// Gets the game time mapped to a 0 to 1 value
    /// </summary>
    public float GameTimeProgress01 { get { return Mathf.InverseLerp(0f, MatchDuration, TimeRemaining); } }

    /// <summary>
    /// Timestamp in milliseconds when match was started by masterclient
    /// </summary>
    public float MatchStartTimeStamp { get; protected set; }

    /// <summary>
    /// Timestamp in milliseconds when ended
    /// </summary>
    public float MatchEndTimeStamp { get; protected set; }

    /// <summary>
    /// Timer used to keep track of game time
    /// </summary>
    public Timer GameTimer { get; protected set; }

    /// <summary>
    /// Time the match has been alive, in milliseconds
    /// </summary>
    public float MatchTimeElapsed => GameTimer.ElapsedTime; //(float)PhotonNetwork.Time - MatchStartTimeStamp; ////

    /// <summary>
    /// How much time is remaining until the game is over, in milliseconds
    /// </summary>
    public float TimeRemaining => GameTimer.TimeRemaining;

    /// <summary>
    /// Timestamp that gets set everytime masterclient initializes a new gamemode, 
    /// is useful if you want to know if the gamemode you disconnected/reconnected from is the same one.
    /// </summary>
    public string GameModeInitializedTimeStamp { get; private set; }

    /// <summary>
    /// Manages points of the gamemode
    /// </summary>
    public virtual IGameModeScoreboard Scoreboard { get; protected set; }

    /// <summary>
    /// Game result on game over
    /// </summary>
    public IGameResult GameResult { get; protected set; }

    public virtual OrdersController OrdersController { get; }

    protected GlobalEventDispatcher globalEventDispatcher;

    protected void Awake()
    {
        GameTimer = new Timer();
        DontDestroyOnLoad(this);
    }

    public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        GameModeService gameModeService = GlobalServiceLocator.Instance.Get<GameModeService>();
        if (gameModeService.CurrentGameMode != this)
            gameModeService.SetGameMode(this);

        PhotonNetworkService.RoomPropertiesChangedEvent += OnRoomPropertiesChangedEvent;
        PhotonNetworkService.PhotonEventReceivedEvent += OnPhotonEventReceived;
    }

    public virtual void OnDestroy()
    {
        PhotonNetworkService.RoomPropertiesChangedEvent -= OnRoomPropertiesChangedEvent;
        PhotonNetworkService.PhotonEventReceivedEvent -= OnPhotonEventReceived;
    }

    public virtual void Setup()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetupRoomProperties();
        }
        else
        {
            Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            MatchDuration = (float)properties[RoomPropertiesPhoton.MATCH_DURATION];
            GameModeInitializedTimeStamp = (string)properties[RoomPropertiesPhoton.GAME_TIME_STAMP];

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.MATCH_START_TIME, out object value))
                MatchStartTimeStamp = (float)value;

            MatchPhase phase = (MatchPhase)(int)properties[RoomPropertiesPhoton.GAME_STATE];
            if (MatchPhase != phase)
                InvokePhase(phase);
        }

        GameTimer.Set(MatchDuration);
    }

    public void OnReconnect()
    {
        Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        MatchDuration = (float)properties[RoomPropertiesPhoton.MATCH_DURATION];
        GameModeInitializedTimeStamp = (string)properties[RoomPropertiesPhoton.GAME_TIME_STAMP];

        MatchPhase phase = (MatchPhase)(int)properties[RoomPropertiesPhoton.GAME_STATE];
        if (MatchPhase != phase)
            InvokePhase(phase);
    }

    /// <summary>
    /// Can the game start?
    /// </summary>
    /// <returns></returns>
    public virtual bool StartRequirementsAreMet()
    {
        // TODO: check all players are ready
        return true;
    }

    protected virtual void SetupRoomProperties()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        GameModeInitializedTimeStamp = DateTime.Now.ToString();
        Dictionary<string, object> properties = new Dictionary<string, object>
        {
            { RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase },
            { RoomPropertiesPhoton.MATCH_DURATION, MatchDuration },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    protected virtual void OnRoomPropertiesChangedEvent(Hashtable properties)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        if (properties.TryGetValue(RoomPropertiesPhoton.MATCH_DURATION, out object newValue))
        {
            MatchDuration = (float)newValue;
            GameTimer.Set(MatchDuration);
        }
        if (properties.TryGetValue(RoomPropertiesPhoton.MATCH_START_TIME, out newValue))
            MatchStartTimeStamp = (float)newValue;
        if (properties.TryGetValue(RoomPropertiesPhoton.GAME_TIME_STAMP, out newValue))
            GameModeInitializedTimeStamp = (string)newValue;
    }

    protected void InvokePhase(MatchPhase phase)
    {
        switch (phase)
        {
            case MatchPhase.PreGame:
                StartPreGame(false);
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
        Debug.Log("New Game phase: " + newPhase);
        MatchPhase = newPhase;
        MatchPhaseChangedEvent?.Invoke(newPhase);
        globalEventDispatcher.Invoke(new GameModePhaseChangedEvent(newPhase));
    }

    public virtual void StartPreGame(bool replay = false)
    {
        SetPhase(MatchPhase.PreGame);
        //if (PhotonNetwork.IsMasterClient)
        //{
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);
            if (replay)
                GameModeInitializedTimeStamp = DateTime.Now.ToString();
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_TIME_STAMP, GameModeInitializedTimeStamp.ToString());
        //}

        if (replay)
        {
            Scoreboard.Reset();
            GameTimer.Reset();
        }
    }

  //  public virtual void StartCountdown()
  //  {
  //      SetPhase(MatchPhase.Countdown);
		//if (PhotonNetwork.IsMasterClient)
  //      {
		//	PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);
  //          RaisePhotonEventCode(PhotonEventCodes.GAME_START);
  //      }
  //      Timer countdownTimer = new Timer();
  //      countdownTimer.Set(countdownDuration);
  //      countdownTimer.Start(StartActiveGame);
  //  }

    public virtual void StartActiveGame()
    {
        if (MatchPhase == MatchPhase.Active)
            return;

        SetPhase(MatchPhase.Active);
        globalEventDispatcher.Invoke(new StartGameEvent());

        //if (PhotonNetwork.IsMasterClient)
        //{
			MatchStartTimeStamp = (float)PhotonNetwork.Time;
            Dictionary<string, object> properties = new Dictionary<string, object>
            {
                { RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase },
                { RoomPropertiesPhoton.MATCH_START_TIME, MatchStartTimeStamp }
            };

            RaisePhotonEventCode(PhotonEventCodes.GAME_START);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        //}

        // In case a player joins mid-game, the start time and current network time have to be subtracted from the match duration 
        float duration = MatchDuration;
        if (MatchStartTimeStamp != 0)
            duration = Mathf.Clamp(MatchDuration - ((float)PhotonNetwork.Time - MatchStartTimeStamp), 0, MatchDuration);

        GameTimer.Set(duration);
        GameTimer.Start(OnTimerReachedZero);
    }

    protected virtual void OnTimerReachedZero()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameTimer.Stop();
            //EndGame();
        }
    }

    public virtual void EndGame()
    {
        SetPhase(MatchPhase.PostGame);
        GameTimer.Stop();

        //if (PhotonNetwork.IsMasterClient)
        //{
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);
            RaisePhotonEventCode(PhotonEventCodes.GAME_STOP);
        //}
        globalEventDispatcher.Invoke(new GameOverEvent(GetGameResult()));
        MatchEndTimeStamp = (float)PhotonNetwork.Time;
    }

    public abstract IGameResult GetGameResult();
    public abstract void DeliverDish(Plate dish);

    public virtual void Shutdown()
    {
        SetPhase(MatchPhase.Undefined);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_STATE, (int)MatchPhase);

        Scoreboard.Dispose();
    }

    public virtual void Replay()
    {
        //if (PhotonNetwork.IsMasterClient)
        RaisePhotonEventCode(PhotonEventCodes.GAME_RESTART);

        if (MatchPhase == MatchPhase.Active)
            EndGame();

        StartPreGame(true);
        Scoreboard.Reset();
        globalEventDispatcher.Invoke(new ReplayEvent());
    }

    public virtual void SetMatchDuration(float newDuration)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.MATCH_DURATION, newDuration);
        MatchDuration = newDuration;
        GameTimer.Set(MatchDuration);
    }

    /// <summary>
    /// Raises a Photon event code for specific game logic such as starting and stopping
    /// </summary>
    /// <param name="code">see<see cref="PhotonEventCodes"/></param>
    /// <param name="receivers"></param>
    protected void RaisePhotonEventCode(int code, object[] content = null, ReceiverGroup receivers = ReceiverGroup.Others)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = receivers };
        PhotonNetwork.RaiseEvent((byte)code, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnPhotonEventReceived(EventData data)
    {
        switch (data.Code)
        {
            case PhotonEventCodes.GAME_START:
                StartActiveGame();
                break;
            case PhotonEventCodes.GAME_STOP:
                EndGame();
                break;
            case PhotonEventCodes.GAME_RESTART:
                Replay();
                break;
        }
    }
}


