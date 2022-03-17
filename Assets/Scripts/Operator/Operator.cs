using Photon.Realtime;
using UnityEngine;
using Utils.Core.Injection;

public class Operator : PhotonClient
{
    public readonly DependencyInjector Injector;
    private GameObject operatorGameObject;

    public Operator(DependencyInjector injector, PhotonNetworkedPlayer networkClient, string ipAddress = "") : base(networkClient, ipAddress)
    {
        Injector = injector;
        injector.RegisterInstance<Operator>(this);

        if (IsLocal)
        {
            networkClient.SetCustomProperty(PlayerPropertiesPhoton.CLIENT_TYPE, (byte)ClientType.Operator);
            InstantiateOperatorUI();
        }
    }

    private void InstantiateOperatorUI()
    {
        // Could do a factory here if a different operator object is needed for a specific platform
        operatorGameObject = Injector.InstantiateGameObject(Resources.Load<GameObject>("Operator"));
        Object.DontDestroyOnLoad(operatorGameObject);
    }

    public override void Dispose()
    {
        base.Dispose();
        Object.Destroy(operatorGameObject);
    }
}
