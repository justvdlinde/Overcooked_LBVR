using Photon.Realtime;

public class StandalonePhotonNetworkService : PhotonNetworkService
{
    protected override void ConnectToSelfHosted(NetworkConfig config)
    {
        StartSearchForLocalServer(config);
    }
}
