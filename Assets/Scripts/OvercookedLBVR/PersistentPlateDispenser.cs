using Photon.Pun;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class PersistentPlateDispenser : Dispenser
{
    public Plate PlateInstance { get; private set; }

    [SerializeField] private bool dispenseOnStart = true;

    private GlobalEventDispatcher globalEventDispatcher = null;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        if (!prefab.TryGetComponent(out Plate plate))
        {
            Debug.LogWarning("Prefab is not of type Plate!, disabling dispenser.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        globalEventDispatcher.Subscribe<StartGameEvent>(OnStartGameEvent);
        globalEventDispatcher.Subscribe<ConnectionSuccessEvent>(OnConnected);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<StartGameEvent>(OnStartGameEvent);
        globalEventDispatcher.Unsubscribe<ConnectionSuccessEvent>(OnConnected);
    }

    protected override GameObject InstantiateObject()
    {
        GameObject instance = base.InstantiateObject();
        if (instance.TryGetComponent(out Plate plate))
            PlateInstance = plate;
        return instance;
    }

    private void OnConnected(ConnectionSuccessEvent obj)
    {
        if (PhotonNetwork.IsMasterClient)
            InstantiateObject();
    }

    private void OnStartGameEvent(StartGameEvent obj)
    {
        if (photonView.IsMine)
            ResetPostion();
    }

    private void ResetPostion()
    {
        if(PlateInstance.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }
}
