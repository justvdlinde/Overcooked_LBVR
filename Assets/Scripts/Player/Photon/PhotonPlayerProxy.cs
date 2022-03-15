using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Proxy object that's synced over network. Instantiates remote player prefab and links it to the remote player.
/// See <see cref="LocalPlayerPawnPhotonManager"/> for more information.
/// </summary>
public class PhotonPlayerProxy : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!info.Sender.IsLocal)
            InstantiateRemotePlayer(info.photonView.InstantiationData, info.Sender);
    }

    private void InstantiateRemotePlayer(object[] data, PhotonNetworkedPlayer sender)
    {
        RemotePlayerPawnFactory factory = new RemotePlayerPawnFactory(data, sender);
        factory.Construct();
    }
}
