using Photon.Pun;
using System;
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
        globalEventDispatcher.Subscribe<ReplayEvent>(OnReplayGameEvent);
        globalEventDispatcher.Subscribe<ConnectionSuccessEvent>(OnConnected);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<StartGameEvent>(OnStartGameEvent);
        globalEventDispatcher.Unsubscribe<ReplayEvent>(OnReplayGameEvent);
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
        if (PhotonNetwork.IsMasterClient)
            ResetPlate();
    }

    private void OnReplayGameEvent(ReplayEvent obj)
    {
        if (PhotonNetwork.IsMasterClient)
            ResetPlate();
    }

    private void ResetPlate()
    {
        if(PlateInstance.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        PlateInstance.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        PlateInstance.Washing.SetPlateClean();
    }
}
