using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class ChoppingProcessable : MonoBehaviourPun
{
    [SerializeField] private Ingredient ingredient = null;
	[SerializeField] private Trigger trigger = null;

	private int currentHitsLeft = 0;
	[SerializeField] private int hitsNeededToProcess = 5;

	[SerializeField] private List<Collider> connectedColliders = new List<Collider>();


	[SerializeField] private bool isChoppable = true;
	[SerializeField] private ParticleSystem particles = null;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		currentHitsLeft = hitsNeededToProcess;
		trigger.EnterEvent += OnEnterEvent;
		trigger.ExitEvent += OnExitEvent;
	}

	private void Disable()
	{
		trigger.EnterEvent -= OnEnterEvent;
		trigger.ExitEvent -= OnExitEvent;
	}

	private void OnEnterEvent(Collider obj)
	{
		if(obj.TryGetComponent<ChoppingCollider>(out ChoppingCollider col))
		{
			foreach (Collider c in connectedColliders)
			{
				col.ToggleCollision(c, true);
			}

			if (isChoppable)
			{
				if (ingredient.Status == IngredientStatus.UnProcessed && currentHitsLeft > 0)
				{
					currentHitsLeft -= col.HitDamage;
					if (particles.isPlaying)
						particles?.Stop();
					particles?.Play();
					//Debug.Log($"Chopped {name} for {col.HitDamage} and has {currentHitsLeft} left");
				}
				else if (ingredient.Status == IngredientStatus.UnProcessed && currentHitsLeft <= 0)
				{
					Chop();
				}
			}
        }
    }

	[Button]
	public void Chop()
    {
		photonView.RPC(nameof(ChopRPC), RpcTarget.All);
	}

	[PunRPC]
	public void ChopRPC(PhotonMessageInfo info)
	{
		ingredient.Process();
		if (particles.isPlaying)
			particles?.Stop();
		particles?.Play();

		Disable();

		if (photonView.IsMine)
		{
			// TO DO: CLEAN THIS UP
			if (ingredient.processToTwoAssets || ingredient.processToCookable)
			{
				PhotonNetwork.Destroy(ingredient.rigidbody.gameObject);
			}
		}
	}

	private void OnExitEvent(Collider obj)
	{
		if (obj.TryGetComponent<ChoppingCollider>(out ChoppingCollider col))
		{
			foreach (Collider c in connectedColliders)
			{
				col.ToggleCollision(c, false);
			}

		}
	}

}
