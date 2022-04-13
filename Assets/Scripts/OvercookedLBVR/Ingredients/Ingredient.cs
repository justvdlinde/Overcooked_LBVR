using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Ingredient : MonoBehaviourPun
{
    public IngredientType IngredientType => ingredientType;
    [SerializeField] private IngredientType ingredientType = IngredientType.None;

    public IngredientStatus Status => status;
    [SerializeField] private IngredientStatus status = IngredientStatus.UnProcessed;

    [Header("References")]
    [SerializeField] private bool needsToBeCooked = false; // reference cookComponent?
    [SerializeField] private IngredientCookController cookComponent = null;

    [Header("Graphics")]
    [SerializeField] private Transform unProcessedGraphics = null;
    [SerializeField] private Transform processedGraphics = null;

    // TODO: place into stackable component
	[SerializeField] private List<GameObject> toggleObjects = new List<GameObject>();

    // TODO: cleanup:
    public bool CanStack = true;
    public Transform recentDishCollider = null;

    private void Awake()
    {
        if (processedGraphics != null && unProcessedGraphics != null)
        {
            unProcessedGraphics.gameObject.SetActive(status == IngredientStatus.UnProcessed);
            processedGraphics.gameObject.SetActive(status == IngredientStatus.Processed);
        }
    }

    private void Update()
    {
        // TODO: place into stackable component
        if (!CanStack && recentDishCollider != null)
		{
            if(Vector3.Distance(recentDishCollider.transform.position, transform.position) > 0.3f)
			{
                CanStack = true;
                recentDishCollider = null;
			}
		}

    }

    // TODO: place into stackable component
	public void SetComponentsOnIngredientActive(bool active)
	{
		foreach (var item in toggleObjects)
		{
			item.SetActive(active);
		}
    }

    public bool IsCookedProperly()
    {
        bool returnValue = Status == IngredientStatus.Processed;
        if (needsToBeCooked)
            returnValue &= cookComponent.State == CookState.Cooked;
        return returnValue;
    }

    public void SetState(IngredientStatus status)
    {
        photonView.RPC(nameof(SetStateRPC), RpcTarget.All, (int)status);
    }

    [PunRPC]
    private void SetStateRPC(int statusIndex)
    {
        status = (IngredientStatus)statusIndex;

        if (processedGraphics != null && unProcessedGraphics != null)
        {
            processedGraphics.gameObject.SetActive(status == IngredientStatus.Processed);
            unProcessedGraphics.gameObject.SetActive(status == IngredientStatus.UnProcessed);
        }
    }

    // TODO: replace with future audio implementation or simple audioSource component
    public void PlaySound(AudioClip clip, Vector3 position)
    {
        GameObject obj = new GameObject();
        obj.transform.position = position;
        obj.AddComponent<AudioSource>();
        obj.GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.2f);
        obj.GetComponent<AudioSource>().PlayOneShot(clip);
        Destroy(obj, clip.length);
        return;
    }
}
