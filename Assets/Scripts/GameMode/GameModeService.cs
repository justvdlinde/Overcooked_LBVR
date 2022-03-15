using Photon.Pun;
using System;
using Utils.Core.Events;
using Utils.Core.Injection;
using Utils.Core.Services;

public class GameModeService : IService
{
    public GameMode CurrentGameMode { get; private set; }

    protected readonly GlobalEventDispatcher globalEventDispatcher;
    protected readonly DependencyInjector injector;

    public GameModeService(GlobalEventDispatcher globalEventDispatcher)
    {
        this.globalEventDispatcher = globalEventDispatcher;
        injector = new DependencyInjector("GamemodeService");
    }

    public void StartNewGame(GameModeEnum gamemode, GameModeSettings settings = null)
    {
        Type type = GameModeHelper.GetGameModeType(gamemode);
        StartNewGame(type, settings);
    }

    public void StartNewGame<T>(GameModeSettings settings = null) where T : GameMode
    {
        StartNewGame(typeof(T), settings);
    }

    public void StartNewGame(Type gamemode, GameModeSettings settings = null)
    {
        GameMode gameModeInstance = injector.CreateType(gamemode) as GameMode;
        SetGameMode(gameModeInstance, settings);
        if(gameModeInstance.MatchPhase == MatchPhase.Undefined)
            CurrentGameMode.PreGame();
    }

    protected void SetGameMode(GameMode gameMode, GameModeSettings settings = null)
    {
        if (CurrentGameMode != null)
        {
            CurrentGameMode.Shutdown();
            CurrentGameMode.Dispose();
        }

        CurrentGameMode = gameMode;
        CurrentGameMode.Setup(settings);
        globalEventDispatcher.Invoke(new GameModeChangedEvent(CurrentGameMode));

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_MODE, (int)CurrentGameMode.GameModeEnum);
    }
}
