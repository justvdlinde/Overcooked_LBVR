using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class SauceRecipient : MonoBehaviourPunCallbacks
{
    public Action<SauceType> SauceTypeChanged;
    public float FillProgressNormalized => fillProgress / fillDuration;
    public SauceType LastReceivedSauceType => lastReceivedSauceType;
    public FoodStack FoodStack => foodStack;

    [Tooltip("Duration in seconds it takes to create sauce")]
	[SerializeField] private float fillDuration = 1;
	[SerializeField] private FoodStack foodStack = null;

    [Header("Prefabs")]
	[SerializeField] private Ingredient ketchupIngredientPrefab = null;
	[SerializeField] private Ingredient mayoIngredientPrefab = null;

	private float fillProgress = 0;
    private float currentSegment = 0;
    private SauceType lastReceivedSauceType = SauceType.Ketchup;

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
        photonView.RPC(nameof(SendSyncDataRPC), player, fillProgress, (int)lastReceivedSauceType);
    }

    [PunRPC]
    private void SendSyncDataRPC(float data, int lastReceivedSauceTypeId)
    {
        fillProgress = data;
        lastReceivedSauceType = (SauceType)lastReceivedSauceTypeId;
    }

    public void ApplySauce(float value, SauceType sauceType)
	{
        if (foodStack.CanPlaceSauce())
        {
            if (lastReceivedSauceType != sauceType)
            {
                fillProgress = 0;
                lastReceivedSauceType = sauceType;
                SauceTypeChanged?.Invoke(lastReceivedSauceType);
            }

            fillProgress += value;
            if (PhotonNetwork.IsMasterClient && fillProgress >= fillDuration)
                CreateSauceIngredient(sauceType);
        }
    }

    public void CreateSauceIngredient(SauceType sauceType)
    {
        if (PhotonNetwork.IsMasterClient)
        {
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
