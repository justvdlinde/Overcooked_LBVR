using System;
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
        INetworkService instance;

        // Use injector.CreateType<PhotonNetworkService>(); for non-monobehaviour services

        GameObject gameObject = new GameObject("[Service] " + nameof(PhotonNetworkService));
        gameObject.SetActive(false);

        Type networkServiceType = null;
        if (GamePlatform.Platform == BuildPlatform.Windows)
            networkServiceType = typeof(StandalonePhotonNetworkService);
        else
            networkServiceType = typeof(MobilePhotonNetworkService);

        instance = gameObject.AddComponent(networkServiceType) as INetworkService;
        injector.InjectGameObject(gameObject);
        gameObject.SetActive(true);

        return instance;
    }
}
