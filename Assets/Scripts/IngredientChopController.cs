using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Extensions;

public class IngredientChopController : MonoBehaviourPun
{
	public bool IsChoppable { get; private set; } = true;

    [SerializeField] private Ingredient ingredient = null;
	[SerializeField] private int hitsNeededToProcess = 5;
	[SerializeField] private List<Collider> connectedColliders = new List<Collider>();
	[SerializeField] private ParticleSystem particles = null;

	[Tooltip("Prefabs to instantiate on Process()")]
	[SerializeField] private TransformPrefabPair[] processInstantiationData = null;

	[Header("Audio")]
	[SerializeField] private AudioSource audioSource = null;
	[SerializeField] private AudioClip[] choppingAudioClips = null;
	[SerializeField, Range(0, 1)] private float pitchDeviation = 0.2f;

	private int hitCount = 0;

	private void Awake()
    {
		IsChoppable = ingredient.Status == IngredientStatus.UnProcessed;
    }

    private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent(out ChoppingCollider choppingCollider))
		{
			EnableColliders(choppingCollider, true);

			if (IsChoppable)
			{
				Chop(choppingCollider.HitDamage);
			}
        }
    }

    private void OnTriggerExit(Collider other)
    {
		if (other.TryGetComponent(out ChoppingCollider choppingCollider))
		{
			EnableColliders(choppingCollider, false);
		}
	}

	private void EnableColliders(ChoppingCollider choppingCollider, bool enable)
    {
		foreach (Collider collider in connectedColliders)
		{
			choppingCollider.ToggleCollision(collider, enable);
		}
	}

	[Button]
	private void ChopDebug()
    {
		Chop(1);
    }

	public void Chop(int hit)
    {
		photonView.RPC(nameof(ChopRPC), RpcTarget.All, hit);
	}

	[PunRPC]
	private void ChopRPC(int hit)
	{
		hitCount += hit;
		PlayChopSound();

		if (particles != null)
		{
			if (particles.isPlaying)
				particles.Stop();
			particles.Play();
		}

		if (PhotonNetwork.IsMasterClient)
		{
			if (hitCount >= hitsNeededToProcess)
			{
				ProcessIngredient();
			}
		}
	}

	[Button]
	public void ProcessIngredient()
    {
		photonView.RPC(nameof(ProcessIngredientRPC), RpcTarget.All);
    }

	[PunRPC]
	private void ProcessIngredientRPC()
    {
		ingredient.SetState(IngredientStatus.Processed);

		for (int i = 0; i < processInstantiationData.Length; i++)
		{
			Transform point = processInstantiationData[i].transform;
			GameObject prefab = processInstantiationData[i].prefab;
			PhotonNetwork.Instantiate(prefab.name, point.position, Quaternion.identity);
		}
		PhotonNetwork.Destroy(ingredient.gameObject);
	}

	private void PlayChopSound()
	{
		audioSource.pitch = Random.Range(1 - pitchDeviation, 1 + pitchDeviation);
		audioSource.PlayOneShot(choppingAudioClips.GetRandom());
	}

	[System.Serializable]
	private class TransformPrefabPair
    {
		public Transform transform;
		public GameObject prefab;
    }
}
