using Photon.Realtime;

public class PhotonClientDisconnectData : IClientDisconnectData
{
    public IClient Client { get; private set; }

    public PhotonClientDisconnectData(PhotonClient client)
    {
        Client = client;
    }
}
