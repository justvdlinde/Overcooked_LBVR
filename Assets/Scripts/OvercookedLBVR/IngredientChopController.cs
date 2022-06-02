using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Extensions;

public class IngredientChopController : MonoBehaviourPun
{
	private enum ChopMethod
    {
		/// <summary>
		/// Instantiates a new prefab when chopped
		/// </summary>
		Instantiate,
		/// <summary>
		/// Simply toggles the corresponding graphics
		/// </summary>
		ToggleGraphic
    }

	public bool IsChoppable => ingredient.State == IngredientStatus.UnProcessed;

	[SerializeField] private Ingredient ingredient = null;
	[SerializeField] private int hitsNeededToProcess = 5;
	[SerializeField] private List<Collider> connectedColliders = new List<Collider>();
	[SerializeField] private ParticleSystem particles = null;

	[Header("Chopping Results")]
	[Tooltip("Decides wether Process() will instantiate new objects or simply toggles graphics")]
	[SerializeField] private ChopMethod chopMethod = ChopMethod.ToggleGraphic;
	[SerializeField] private Transform unProcessedGraphics = null;
	[SerializeField] private Transform processedGraphics = null;
	[SerializeField] private TransformPrefabPair[] processInstantiationData = null;

	[Header("Audio")]
	[SerializeField] private AudioSource audioSource = null;
	[SerializeField] private AudioClip[] choppingAudioClips = null;
	[SerializeField, Range(0, 1)] private float pitchDeviation = 0.2f;

	private int hitCount = 0;

	public bool doNotChop = false;

    private void OnValidate()
    {
		if (ingredient == null)
			ingredient = transform.GetComponentInParent<Ingredient>();
    }

    private void Awake()
    {
		// TODO: test if these work when joining late:
		if(!doNotChop)
			ToggleGraphicsToState();
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent(out ChoppingCollider choppingCollider))
		{
			EnableColliders(choppingCollider, true);

			if (IsChoppable && doNotChop)
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
		if(ingredient.StatusConditionManager.CanChop)
			if(photonView.IsMine)
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

		if (hitCount >= hitsNeededToProcess)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				ProcessIngredient();
			}
		}
	}

	[Button]
	public void ProcessIngredient()
    {
		if (doNotChop)
			return;

		ingredient.SetState(IngredientStatus.Processed);
		photonView.RPC(nameof(ProcessIngredientRPC), RpcTarget.All);
    }

	[PunRPC]
	private void ProcessIngredientRPC()
	{
		Debug.Log("ProcessIngredientRPC");

		if (PhotonNetwork.IsMasterClient)
			ingredient.SetState(IngredientStatus.Processed);

		if (chopMethod == ChopMethod.Instantiate)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				for (int i = 0; i < processInstantiationData.Length; i++)
				{
					Transform point = processInstantiationData[i].transform;
					GameObject prefab = processInstantiationData[i].prefab;
					GameObject instance = PhotonNetwork.Instantiate(prefab.name, point.position, Quaternion.identity);
					IngredientStatusCondition ingredientComponent = instance.GetComponentInChildren<IngredientStatusCondition>();
					if (ingredientComponent != null)
						ingredientComponent.CopyValues(ingredient.StatusConditionManager);
				}
			}

			if(photonView.IsMine)
				PhotonNetwork.Destroy(ingredient.gameObject);
		}
		else
		{
			ToggleGraphicsToState();
		}
	}
	
	private void ToggleGraphicsToState()
    {
		Debug.Log("ToggleGraphicsToState");
		if (chopMethod == ChopMethod.ToggleGraphic)
		{
			Debug.Log(chopMethod);
			if (processedGraphics != null && unProcessedGraphics != null)
			{
				Debug.Log("toggle visuals");
				unProcessedGraphics.gameObject.SetActive(ingredient.State == IngredientStatus.UnProcessed);
				processedGraphics.gameObject.SetActive(ingredient.State == IngredientStatus.Processed);
			}
		}
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
