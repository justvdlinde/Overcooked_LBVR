using Photon.Realtime;
using Utils.Core.Events;
using Utils.Core.Injection;

public class SpectatorFactory : IFactory<Spectator>
{
    private readonly PhotonNetworkedPlayer photonNetworkedPlayer;

    public SpectatorFactory(PhotonNetworkedPlayer photonNetworkedPlayer)
    {
        this.photonNetworkedPlayer = photonNetworkedPlayer;
    }

    public Spectator Construct()
    {
        // Here we can potentially differentiate between Windows and Android(using AR for example), and create the appropiate spectator class

        DependencyInjector injector = new DependencyInjector("Spectator");
        string ipAddress = photonNetworkedPlayer.IsLocal ? NetworkHelper.GetLocalIPAddress().ToString() : "";
        EventDispatcher localEventDispatcher = new EventDispatcher("Spectator");
        injector.RegisterInstance<EventDispatcher>(localEventDispatcher);

        Spectator spectator = new Spectator(injector, photonNetworkedPlayer, ipAddress);
        injector.RegisterInstance<Spectator>(spectator);
        return spectator;
    }
}
