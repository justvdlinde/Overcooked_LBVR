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

    public void StartNewGame(GameModeEnum gamemode)
    {
        Type type = GameModeHelper.GetGameModeType(gamemode);
        StartNewGame(type);
    }

    public void StartNewGame<T>() where T : GameMode
    {
        StartNewGame(typeof(T));
    }

    public void StartNewGame(Type gamemode)
    {
        GameMode gameModeInstance = injector.CreateType(gamemode) as GameMode;
        SetGameMode(gameModeInstance);
        if(gameModeInstance.MatchPhase == MatchPhase.Undefined)
            CurrentGameMode.PreGame();
    }

    protected void SetGameMode(GameMode gameMode)
    {
        if (CurrentGameMode != null)
        {
            CurrentGameMode.Shutdown();
            CurrentGameMode.Dispose();
        }

        CurrentGameMode = gameMode;
        CurrentGameMode.Setup();
        globalEventDispatcher.Invoke(new GameModeChangedEvent(CurrentGameMode));

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_MODE, (int)CurrentGameMode.GameModeEnum);
    }
}
