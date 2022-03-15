using Photon.Pun;
using UnityEngine;
using Utils.Core.Services;

/// <summary>
/// Initializes important classes that need to be setup before the game starts.
/// </summary>
public class GameInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeMethodLoad()
    {
        // These services need to be initialized at startup, simply calling them is enough to be initialized
        GlobalServiceLocator.Instance.Get<INetworkService>();
        GlobalServiceLocator.Instance.Get<PlayersManager>();
        PhotonNetwork.OfflineMode = true;
    }
}
