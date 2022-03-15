using Photon.Pun;
using UnityEngine;
using Utils.Core.Services;

public class DummyPlayerPawnPhotonManager : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private PlayerPawn playerPawn = null;

    private PlayersManager playersManager;

    private void Awake()
    {
        playersManager = GlobalServiceLocator.Instance.Get<PlayersManager>();    
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (info.Sender.IsLocal)
            return;

        object[] data = info.photonView.InstantiationData;
        string playerId = (string)data[0];
        IPlayer player = playersManager.CreateDummyPlayer(playerId, false);
        playerPawn.Setup(player);
        player.SetPlayerPawn(playerPawn);
    }

    public void OnDestroy()
    {
        if (playersManager.AllPlayers.Contains(playerPawn.Owner))
            playersManager.RemovePlayer(playerPawn.Owner);
    }
}
