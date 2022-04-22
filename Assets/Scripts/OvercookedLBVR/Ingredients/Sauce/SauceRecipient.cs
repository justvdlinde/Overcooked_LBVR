using Photon.Pun;
using UnityEngine;

public class SauceRecipient : MonoBehaviourPun
{
    [Tooltip("Duration in seconds it takes to create sauce")]
	[SerializeField] private float fillDuration = 1;
	[SerializeField] private FoodStack foodStack = null;

    [Header("Prefabs")]
	[SerializeField] private Ingredient ketchupIngredientPrefab = null;
	[SerializeField] private Ingredient mayoIngredientPrefab = null;

	private float fillProgress = 0;
    private float currentSegment = 0;
    private SauceType lastReceivedSauceType;

    // TODO: sync fillProgress
	public void ApplySauce(float value, SauceType sauceType)
	{
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("apply sauce");
            if (foodStack.CanPlaceSauce(sauceType))
            {
                fillProgress += value;
                Debug.Log(fillProgress);
                lastReceivedSauceType = sauceType;

                if (fillProgress >= fillDuration)
                    AddSauceIngredient(sauceType);
            }
        }
    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // TODO: send progressm make sure not to overwrite local player's progress but add

        if (stream.IsWriting)
        {
            //stream.SendNext(x);
        }
        else
        {
            //x = (int)stream.ReceiveNext();
        }
    }

    public void AddSauceIngredient(SauceType sauceType)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("AddSauceIngredient");
            GameObject ingredientPrefab = sauceType == SauceType.Ketchup ? ketchupIngredientPrefab.gameObject : mayoIngredientPrefab.gameObject;

            object[] initData = new object[] { foodStack.photonView.ViewID };
            GameObject instance = PhotonNetwork.Instantiate(ingredientPrefab.name, transform.position, transform.rotation, data: initData);
            foodStack.AddIngredientToStack(instance.GetComponent<Ingredient>());
            ResetProgress();
        }
    }

    public void ResetProgress()
    {
        photonView.RPC(nameof(ResetProgressRPC), RpcTarget.All);
    }

    [PunRPC] 
    private void ResetProgressRPC()
    {
        fillProgress = 0f;
    }
}
