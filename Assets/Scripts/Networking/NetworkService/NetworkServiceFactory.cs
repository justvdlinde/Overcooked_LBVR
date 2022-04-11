using UnityEngine;
using Utils.Core.Injection;
using Utils.Core.Services;

public class NetworkServiceFactory : IServiceFactory<INetworkService>
{
    private readonly DependencyInjector injector;

    public NetworkServiceFactory(DependencyInjector injector)
    {
        this.injector = injector;
    }

    public INetworkService Construct()
    {
        // Use injector.CreateType<PhotonNetworkService>(); for non-monobehaviour services

        INetworkService instance;
        GameObject gameObject = new GameObject("[Service] " + nameof(PhotonNetworkService));
        gameObject.SetActive(false);
        instance = gameObject.AddComponent(typeof(PhotonNetworkService)) as INetworkService;
        injector.InjectGameObject(gameObject);
        gameObject.SetActive(true);

        return instance;
    }
}
