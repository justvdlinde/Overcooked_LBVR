using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnAssetCollider : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		ReturnableAsset asset = other.GetComponentInParent<ReturnableAsset>();

		if(asset != null)
		{
			asset.Return();
			return;
		}

		PhotonView photonView = other.GetComponentInParent<PhotonView>();
		if(photonView == null)
			photonView = other.GetComponentInChildren<PhotonView>();

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
