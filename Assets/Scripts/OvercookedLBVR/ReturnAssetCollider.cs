using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class ReturnAssetCollider : MonoBehaviour
{
	private GlobalEventDispatcher globalEventdispatcher = null;

	private void Awake()
	{
		globalEventdispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerPawn>())
			return;

		ReturnableAsset asset = other.GetComponentInParent<ReturnableAsset>();

		if (asset != null)
		{
			asset.Return();
			return;
		}

		PhotonView photonView = other.GetComponentInParent<PhotonView>();
		if (photonView.GetComponent<PlayerPawn>() )
			return;

		if(photonView == null)
			photonView = other.GetComponentInChildren<PhotonView>();
		Plate plate = other.GetComponentInParent<Plate>();

		if(plate != null)
		{
			if (!photonView.IsMine)
				return;

			if (globalEventdispatcher != null)
				globalEventdispatcher.Invoke<PlateDestroyedEvent>(new PlateDestroyedEvent());

			PhotonNetwork.Destroy(photonView.transform.gameObject);

			return; 
		}

		if (asset == null && photonView != null)
		{
			if (!photonView.IsMine)
				return;
			// TO DO: replace RB check with IPoolable
			// TO DO: destroy photon object
			//Destroy(rb.transform.gameObject);
			PhotonNetwork.Destroy(photonView.transform.gameObject);
			return;
		}
	}
}
