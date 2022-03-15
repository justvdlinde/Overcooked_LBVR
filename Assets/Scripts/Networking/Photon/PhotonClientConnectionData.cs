public class PhotonClientConnectionData : IClientConnectionData
{
    public IClient Client { get; private set; }

    public PhotonClientConnectionData(PhotonClient client)
    {
        Client = client;
    }
}
