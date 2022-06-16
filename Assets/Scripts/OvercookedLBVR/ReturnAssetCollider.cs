using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class ReturnAssetCollider : MonoBehaviourPun
{
	private GlobalEventDispatcher globalEventdispatcher = null;

	[SerializeField] private ParticleSystem despawnParticles = null;

	[SerializeField] private BoxCollider connectedCollider = null;

	[SerializeField] private GameModeService gameModeService = null;

	private void Awake()
	{
		globalEventdispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
		gameModeService = GlobalServiceLocator.Instance.Get<GameModeService>();
	}

	private bool IsMatchActive()
	{
		if (gameModeService == null)
			gameModeService = GlobalServiceLocator.Instance.Get<GameModeService>();
		if (gameModeService == null)
			return false;
		return gameModeService.IsMatchActive();
	}

	private void OnTriggerEnter(Collider other)
	{

		if (other.GetComponent<PlayerPawn>())
			return;

		ReturnableAsset asset = other.GetComponentInParent<ReturnableAsset>();

		if (asset != null)
		{
			asset.Return();
			photonView.RPC(nameof(PlayParticlesRPC), RpcTarget.All, asset.transform.position);

			return;
		}

		if (!IsMatchActive())
			return;

		PhotonView otherPhotonView = other.GetComponentInParent<PhotonView>();
		if (otherPhotonView == null || otherPhotonView.GetComponent<PlayerPawn>())
			return;

		if(otherPhotonView == null)
			otherPhotonView = other.GetComponentInChildren<PhotonView>();
		Plate plate = other.GetComponentInParent<Plate>();

		if(plate != null)
		{
			if (!otherPhotonView.IsMine)
				return;

			photonView.RPC(nameof(PlayParticlesRPC), RpcTarget.All, otherPhotonView.transform.position);

			PhotonNetwork.Destroy(otherPhotonView.transform.gameObject);

			return; 
		}

		if (asset == null && otherPhotonView != null)
		{
			if (!otherPhotonView.IsMine)
				return;
			// TO DO: replace RB check with IPoolable
			// TO DO: destroy photon object
			//Destroy(rb.transform.gameObject);

			photonView.RPC(nameof(PlayParticlesRPC), RpcTarget.All, otherPhotonView.transform.position);

			PhotonNetwork.Destroy(otherPhotonView.transform.gameObject);
			return;
		}
	}

	[PunRPC]
	private void PlayParticlesRPC(Vector3 position)
	{
		if (despawnParticles == null)
			return;
		despawnParticles.transform.position = position;
		despawnParticles.Play();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red * new Color(1,1,1,0.3f);

		Gizmos.DrawCube(transform.position, connectedCollider.bounds.size);

	}
}
