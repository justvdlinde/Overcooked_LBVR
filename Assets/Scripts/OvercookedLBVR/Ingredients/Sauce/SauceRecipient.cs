using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SauceRecipient : MonoBehaviourPunCallbacks
{
    public float FillProgressNormalized => fillProgress / fillDuration;

    [Tooltip("Duration in seconds it takes to create sauce")]
	[SerializeField] private float fillDuration = 1;
	[SerializeField] private FoodStack foodStack = null;

    [Header("Prefabs")]
	[SerializeField] private Ingredient ketchupIngredientPrefab = null;
	[SerializeField] private Ingredient mayoIngredientPrefab = null;

	private float fillProgress = 0;
    private float currentSegment = 0;
    private SauceType lastReceivedSauceType;

    public override void OnEnable()
    {
        base.OnEnable();
        foodStack.IngredientAddedEvent += OnIngredientAddedToFoodStackEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        foodStack.IngredientAddedEvent -= OnIngredientAddedToFoodStackEvent;
    }

    public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            SendSyncData(newPlayer);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        photonView.RPC(nameof(SendSyncDataRPC), player, fillProgress);
    }

    [PunRPC]
    private void SendSyncDataRPC(object data)
    {
        fillProgress = (float)data;
    }

    // TODO: do something when a different sauceType is used then lastReceivedSauceType
    public void ApplySauce(float value, SauceType sauceType)
	{
        if (PhotonNetwork.IsMasterClient)
        {
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

    private void OnIngredientAddedToFoodStackEvent(Ingredient obj)
    {
        ResetProgress();
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
