using Photon.Pun;
using System;

public static class PhotonGameModeHelper
{
    /// <summary>
    /// Callback for when the server's gamemode has changed 
    /// </summary>
    public static Action<GameModeEnum> ServerGameModeChangedEvent;

    static PhotonGameModeHelper()
    {
        PhotonNetworkService.RoomPropertiesChangedEvent += OnRoomPropertiesUpdateEvent;
    }

    /// <summary>
    /// Does the current room have a gamemode running?
    /// </summary>
    /// <param name="gameMode"></param>
    /// <returns></returns>
    public static bool ServerHasGameMode(out GameModeEnum gameMode)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.GAME_MODE, out object gameModeIndex))
        {
            gameMode = (GameModeEnum)(int)gameModeIndex;
            return true;
        }
        else
        {
            gameMode = GameModeEnum.Story;
            return false;
        }
    }

    private static void OnRoomPropertiesUpdateEvent(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties.TryGetValue(RoomPropertiesPhoton.GAME_MODE, out object property))
        {
            GameModeEnum newGameMode = (GameModeEnum)(int)property;
            ServerGameModeChangedEvent?.Invoke(newGameMode);
        }
    }

    /// <summary>
    /// Compares the gamemode on the server with the current gamemode and returns wether they're equal
    /// </summary>
    /// <returns></returns>
    public static bool ServerGamemodeEqualsCurrentGamemode(GameMode currentGameMode)
    {
        if (PhotonNetwork.CurrentRoom == null || currentGameMode == null)
            return false;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.GAME_TIME_STAMP, out object timeStampObject))
            return (string)timeStampObject == currentGameMode.GameModeInitializedTimeStamp;
        else
            return false;
    }
}
