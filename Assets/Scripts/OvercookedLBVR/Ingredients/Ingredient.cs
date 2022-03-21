using Photon.Pun;
using PhysicsCharacter;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public IngredientType ingredientType = IngredientType.None;
    public IngredientStatus status = IngredientStatus.UnProcessed;
    public new Rigidbody rigidbody = null;

    public bool needsToBeCooked = false;
    public CookComponent cookComponent = null;

    public Transform unProcessedGraphics = null;
    public Transform processedGraphics = null;

    public bool processToTwoAssets = false;
    [SerializeField] private GameObject result1 = null;
    [SerializeField] private GameObject result2 = null;

    public bool processToCookable = false;
    [SerializeField] private GameObject cookable = null;

    private void Awake()
	{
        unProcessedGraphics?.gameObject.SetActive(true);
        processedGraphics?.gameObject.SetActive(false);
    }

    public void DisableComponentsOnDish()
	{
        PickupableObject[] handles = transform.parent.parent.gameObject.GetComponentsInChildren<PickupableObject>();

		foreach (var item in handles)
		{
            item.gameObject.SetActive(false);
		}

        AudioPlayerScript[] audioPlayers = transform.parent.parent.gameObject.GetComponentsInChildren<AudioPlayerScript>();

        foreach (var item in audioPlayers)
        {
            item.enabled = false;
        }
    }

    public void Process()
	{
        // TO DO: CLEAN THIS UP
        if(!processToTwoAssets && !processToCookable)
		{
            unProcessedGraphics.gameObject.SetActive(false);
            processedGraphics.gameObject.SetActive(true);
		}
        else if(processToTwoAssets)
		{
            unProcessedGraphics.gameObject.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                if(result1 != null)
                    PhotonNetwork.Instantiate(result1.name, unProcessedGraphics.transform.position, Quaternion.identity);
                if(result2 != null)
                    PhotonNetwork.Instantiate(result2.name, unProcessedGraphics.transform.position + Vector3.up * 0.05f, Quaternion.identity);
            }
            //GameObject r1 = Instantiate(result1);
            //r1.transform.position = unProcessedGraphics.transform.position;
            //GameObject r2 = Instantiate(result2);
            //r2.transform.position = r1.transform.position + Vector3.up * 0.05f;
        }
        else if(processToCookable)
		{
            unProcessedGraphics.gameObject.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Instantiate(cookable.name, unProcessedGraphics.transform.position, Quaternion.identity);
		}
        status = IngredientStatus.Processed;
	}

    public void TogglePhysics(bool toggle)
    {
        rigidbody.isKinematic = !toggle;
        rigidbody.useGravity = toggle;
    }

    public bool IsCookedProperly()
    {
        bool returnValue = status == IngredientStatus.Processed;
        if (needsToBeCooked)
            returnValue &= cookComponent.status == CookStatus.Cooked;
        return returnValue;
    }

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
