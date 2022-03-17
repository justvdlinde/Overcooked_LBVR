using Photon.Pun;
using System.Collections;
using UnityEngine;
using Utils.Core;
using Utils.Core.Services;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private bool connectToNetwork = false;
    [Tooltip("Only required if connectToNetwork is set to true")]
    [SerializeField] private NetworkConfig networkConfig = null;

    [field: SerializeField]
    private bool usePhysicsPlayer = false;

    private INetworkService networkService;

    protected virtual void Awake()
    {
        PlayersManager playersManager = GlobalServiceLocator.Instance.Get<PlayersManager>();
        if (playersManager.LocalPlayer != null)
        {
            Debug.LogWarning("Local Player is already present, returning code");
            gameObject.SetActive(false);
            return;
        }

        networkService = GlobalServiceLocator.Instance.Get<INetworkService>();

        PlayerFactory factory = new PlayerFactory(PhotonNetwork.LocalPlayer);
        IPlayer LocalPlayer = factory.Construct();
        networkService.AddClient(LocalPlayer as IClient);
        LocalPlayer.Pawn.transform.position = transform.position;
    }

    // Needs to be IEnumerator, as connecting through Photon will otherwise not work when called from Awake or Start
    protected IEnumerator Start()
    {
        yield return null;
        if (connectToNetwork)
        {
            if (networkConfig != null)
                networkService.Connect(networkConfig);
            else
                Debug.LogError("Networkconfig cannot be null when trying to connect!");
        }

        gameObject.SetActive(false);
    }

    protected virtual void OnDrawGizmos()
    {
        GizmosUtility.DrawWireBox(transform, transform.position + Vector3.up, new Vector3(1, 2, 1), Color.red);
    }
}
