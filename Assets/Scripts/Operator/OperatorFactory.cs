using Photon.Realtime;
using Utils.Core.Events;
using Utils.Core.Injection;

public class OperatorFactory : IFactory<Operator>
{
    private readonly PhotonNetworkedPlayer photonNetworkedPlayer;
    private DependencyInjector injector;

    public OperatorFactory(PhotonNetworkedPlayer photonNetworkedPlayer)
    {
        this.photonNetworkedPlayer = photonNetworkedPlayer;
    }

    public OperatorFactory(DependencyInjector injector, PhotonNetworkedPlayer photonNetworkedPlayer) : this(photonNetworkedPlayer)
    {
        this.injector = injector;
    }

    public Operator Construct()
    {
        if (injector == null)
            injector = new DependencyInjector("Remote Operator");

        string ipAddress = photonNetworkedPlayer.IsLocal ? NetworkHelper.GetLocalIPAddress().ToString() : "";
        EventDispatcher localEventDispatcher = new EventDispatcher("Operator");
        injector.RegisterInstance<EventDispatcher>(localEventDispatcher);

        OperatorControls input = new OperatorControls();
        input.Enable();
        injector.RegisterInstance<OperatorControls>(input);

        Operator @operator = new Operator(injector, photonNetworkedPlayer, ipAddress);
        return @operator;
    }
}
