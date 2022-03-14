using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utils.Core.Services;

public class RemotePlayerPawnFactory : IFactory<PlayerPawn>
{
    private readonly object[] data;
    private readonly PhotonNetworkedPlayer sender;
    private readonly PlayersManager playersManager;

    public RemotePlayerPawnFactory(object[] data, PhotonNetworkedPlayer sender)
    {
        this.data = data;
        this.sender = sender;
        playersManager = GlobalServiceLocator.Instance.Get<PlayersManager>();
    }

    public PlayerPawn Construct()
    {
        // Here we can create different prefabs for different platforms, such as Spree

        Vector3 pos = (Vector3)data[0];
        Quaternion rot = (Quaternion)data[1];
        string playerId = (string)data[2];
        int photonviewId = (int)data[3];

        IPlayer owner = playersManager.GetPlayerById(playerId);
        GameObject instance = owner.Injector.InstantiateGameObject((GameObject)Resources.Load("RemotePlayerPawn"));

        Object.DontDestroyOnLoad(instance);
        instance.transform.position = pos;
        instance.transform.rotation = rot;
        instance.transform.SetParent(instance.transform);

        PhotonView photonView = instance.GetComponent<PhotonView>();
        photonView.ViewID = photonviewId;
        photonView.TransferOwnership(sender);

        PlayerPawn pawn = instance.GetComponent<PlayerPawn>();
        pawn.Setup(owner);
        owner.SetPlayerPawn(pawn);

        return pawn;
    }
}
