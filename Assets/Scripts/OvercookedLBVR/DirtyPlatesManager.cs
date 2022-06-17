using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;

public class DirtyPlatesManager : MonoBehaviourPun
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private AudioSource AudioSource;

    [SerializeField] private ConfigurableJoint joint = null;
    [SerializeField] private Collider checkCollider = null;
    [SerializeField] private LayerMask layermask = 0;

    private GlobalEventDispatcher globalEventDispatcher = null;
	[SerializeField] private float dishRespawnDelay = 10f;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnEnable()
	{
        globalEventDispatcher.Subscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
        globalEventDispatcher.Subscribe<PlateOutOfBoundsEvent>(OnPlateOutOfBoundsEvent);
    }

	private void OnDisable()
	{
        globalEventDispatcher.Unsubscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
        globalEventDispatcher.Unsubscribe<PlateOutOfBoundsEvent>(OnPlateOutOfBoundsEvent);
    }

	private void OnPlateOutOfBoundsEvent(PlateOutOfBoundsEvent @event)
	{
        photonView.RPC(nameof(StartDelayRPC), RpcTarget.All, @event.Plate.photonView.ViewID);
    }

    private void OnDishDeliveredEvent(DishDeliveredEvent @event)
	{
        if(@event.Dish != null)
            photonView.RPC(nameof(StartDelayRPC), RpcTarget.All, @event.Dish.photonView.ViewID);
	}

    [PunRPC]
    private void StartDelayRPC(int plateViewId)
    {
        StartCoroutine(Delay(plateViewId));
    }

    private IEnumerator Delay(int plateViewId)
	{
        yield return new WaitForSeconds(dishRespawnDelay);

        if (PhotonNetwork.IsMasterClient)
        {
            Plate plate = PhotonView.Find(plateViewId).GetComponent<Plate>();
            AddToDirtyDishes(plate);
        }
	}

	private void Update()
	{
        Collider[] cols = Physics.OverlapBox(checkCollider.bounds.center, checkCollider.bounds.extents, checkCollider.transform.rotation, layermask);

        int colsCount = -1;
        float colHeight = 0.0225f;

        for (int i = 0; i < cols.Length; i++)
		{
            if (cols[i].isTrigger && cols[i].tag != Tags.PLATE)
                continue;

            //colHeight = cols[i].bounds.size.y;
            colsCount++;
		}

        Vector3 targPos = joint.targetPosition;
        targPos.y = Mathf.Max(0, colsCount) * colHeight;
        joint.targetPosition = targPos;
	}

	public void AddToDirtyDishes(Plate plate)
    {
        plate.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        plate.Washing.SetPlateDirty();
        plate.SetActive(true);
        photonView.RPC(nameof(PlayAudioRPC), RpcTarget.All);
        photonView.TransferOwnership(-1);
    }

    [PunRPC]
    private void PlayAudioRPC(PhotonMessageInfo info)
    {
        AudioSource.Play();
    }
}
