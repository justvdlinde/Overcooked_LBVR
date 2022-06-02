using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;

public class DirtyPlateSpawner : MonoBehaviourPun
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private AudioSource AudioSource;

    [SerializeField] private ConfigurableJoint joint = null;
    [SerializeField] private Collider checkCollider = null;
    [SerializeField] private LayerMask layermask = 0;

    private GlobalEventDispatcher globalEventDispatcher = null;
	[SerializeField] private float dishRespawnDelay = 10f;

	private void OnEnable()
	{
        if (globalEventDispatcher == null)
            globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        globalEventDispatcher.Subscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
        globalEventDispatcher.Subscribe<PlateDestroyedEvent>(OnPlateDestroyedEvent);
    }

	private void OnDisable()
	{
        globalEventDispatcher.Unsubscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
        globalEventDispatcher.Unsubscribe<PlateDestroyedEvent>(OnPlateDestroyedEvent);
    }

	private void OnPlateDestroyedEvent(PlateDestroyedEvent obj)
	{
        photonView.RPC(nameof(PlateDestroyedRPC), RpcTarget.All);
    }

    [PunRPC]
    private void PlateDestroyedRPC()
	{
        StartCoroutine(SpawnDishInDelay());
    }

    private void OnDishDeliveredEvent(DishDeliveredEvent _event)
	{
        photonView.RPC(nameof(DishDeliveredRPC), RpcTarget.All);
	}

    [PunRPC]
    private void DishDeliveredRPC()
	{
        StartCoroutine(SpawnDishInDelay());
	}

    private IEnumerator SpawnDishInDelay()
	{
        yield return new WaitForSeconds(dishRespawnDelay);

        if (PhotonNetwork.IsMasterClient)
            DispenseObject();
	}

	private void Update()
	{
        Collider[] cols = Physics.OverlapBox(checkCollider.bounds.center, checkCollider.bounds.extents, checkCollider.transform.rotation, layermask);

        int colsCount = -1;
        float colHeight = 0.0225f;

        for (int i = 0; i < cols.Length; i++)
		{
            if (cols[i].isTrigger && cols[i].tag != "plate")
                continue;

            //colHeight = cols[i].bounds.size.y;
            colsCount++;
		}

        Vector3 targPos = joint.targetPosition;
        targPos.y = Mathf.Max(0, colsCount) * colHeight;
        joint.targetPosition = targPos;
	}

	public void DispenseObject(Rigidbody obj)
    {
        GameObject g = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        g.AddComponent(typeof(DestroyOnGameStart));
        photonView.RPC(nameof(DispenseObjectRPC), RpcTarget.All);
        photonView.TransferOwnership(-1);

        //instance.AddForce(instance.transform.forward * forceAmount, forceMode);
    }

    [PunRPC]
    private void DispenseObjectRPC(PhotonMessageInfo info)
    {
        AudioSource.Play();
    }

    [Button]
    public void DispenseObject()
    {
        DispenseObject(prefab);

    }
}
