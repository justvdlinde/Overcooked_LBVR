public class PlayerFlow : GameFlow
{
    public IPlayer LocalPlayer { get; protected set; }

    protected override void Awake()
    {
        CreatePlayer();
        base.Awake();
    }

    protected virtual void CreatePlayer()
    {
        PlayerFactory factory = new PlayerFactory(injector, Photon.Pun.PhotonNetwork.LocalPlayer);
        LocalPlayer = factory.Construct();
        networkService.AddClient(LocalPlayer as IClient);
    }
}
