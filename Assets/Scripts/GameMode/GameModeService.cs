using Photon.Pun;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class GameModeService : IService
{
    public GameMode CurrentGameMode { get; private set; }

    protected readonly GlobalEventDispatcher globalEventDispatcher;

    public GameModeService(GlobalEventDispatcher globalEventDispatcher)
    {
        this.globalEventDispatcher = globalEventDispatcher;
    }

    public void StartNewGame(GameModeEnum gamemode)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameMode gameModeInstance = PhotonNetwork.InstantiateSceneObject(GameModeHelper.GetGameModePrefabName(gamemode), Vector3.zero, Quaternion.identity).GetComponent<GameMode>();
            Object.DontDestroyOnLoad(gameModeInstance);
            SetGameMode(gameModeInstance);
        }
    }

    public void SetGameMode(GameMode gameMode)
    {
        if (CurrentGameMode == gameMode)
            return;

        if (CurrentGameMode != null)
        {
            CurrentGameMode.Shutdown();
            if(PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(CurrentGameMode.gameObject);
        }

        CurrentGameMode = gameMode;
        CurrentGameMode.Setup();
        if (CurrentGameMode.MatchPhase == MatchPhase.Undefined)
            CurrentGameMode.StartPreGame();

        globalEventDispatcher.Invoke(new GameModeChangedEvent(CurrentGameMode));

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.GAME_MODE, (int)CurrentGameMode.GameModeEnum);
    }

    public bool IsMatchActive()
    {
        if (CurrentGameMode == null)
            return false;

        return CurrentGameMode.MatchPhase == MatchPhase.Active;
    }
}
