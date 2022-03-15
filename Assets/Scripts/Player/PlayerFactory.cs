using Photon.Realtime;
using Utils.Core.Events;
using Utils.Core.Injection;

public class PlayerFactory : IFactory<IPlayer>
{
    private readonly PhotonNetworkedPlayer photonNetworkedPlayer;
    private DependencyInjector injector;

    public PlayerFactory(PhotonNetworkedPlayer photonNetworkedPlayer)
    {
        this.photonNetworkedPlayer = photonNetworkedPlayer;
    }

    public PlayerFactory(DependencyInjector injector, PhotonNetworkedPlayer photonNetworkedPlayer) : this(photonNetworkedPlayer)
    {
        this.injector = injector;
    }

    public IPlayer Construct()
    {
        // Here we can also potentially create a new SpreePlayer for example when on Spree platform

        if (injector == null)
            injector = new DependencyInjector("Remote Player");

        string ipAddress = photonNetworkedPlayer.IsLocal ? NetworkHelper.GetLocalIPAddress().ToString() : "";
        EventDispatcher localEventDispatcher = new EventDispatcher("Player");
        injector.RegisterInstance<EventDispatcher>(localEventDispatcher);

        CreateInput();

        PhotonPlayer player = new PhotonPlayer(injector, photonNetworkedPlayer, ipAddress, "Player");
        return player;
    }

    protected void CreateInput()
    {
        if (photonNetworkedPlayer.IsLocal)
        {
            PlayerControls input = new PlayerControls();
            injector.RegisterInstance<PlayerControls>(input);
            input.Enable();
        }
        else
        {
            RemotePlayerInput input = new RemotePlayerInput();
            injector.RegisterInstance<RemotePlayerInput>(input);
        }
    }
}
