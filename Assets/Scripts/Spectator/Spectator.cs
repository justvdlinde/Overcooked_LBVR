using Photon.Realtime;
using Utils.Core.Injection;

public class Spectator : PhotonClient
{
    public readonly DependencyInjector Injector;

    public Spectator(DependencyInjector injector, PhotonNetworkedPlayer networkClient, string ipAddress = "") : base(networkClient, ipAddress)
    {
        Injector = injector;

        if (IsLocal)
        {
            networkClient.SetCustomProperty(PlayerPropertiesPhoton.CLIENT_TYPE, (byte)ClientType.Spectator);
        }
    }
}
