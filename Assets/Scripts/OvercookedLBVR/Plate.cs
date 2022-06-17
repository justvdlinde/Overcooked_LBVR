using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

[SelectionBase]
public class Plate : MonoBehaviourPun
{
    public const int DISH_MIN_INGREDIENTS = 3;

    public FoodStack FoodStack => foodStack;
    public PlateWashing Washing => washing;

    [SerializeField] private FoodStack foodStack = null;
    [SerializeField] private PlateWashing washing = null;

    private GlobalEventDispatcher globalEventdispatcher;
 
    private void Awake()
    {
        globalEventdispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
        globalEventdispatcher.Subscribe<PlayerJoinEvent>(OnPlayerJoinEvent);
    }

    private void OnDestroy()
    {
        globalEventdispatcher.Unsubscribe<PlayerJoinEvent>(OnPlayerJoinEvent);
    }

    // Photon OnPlayerEnteredRoom does not work for some reason so this event is used
    private void OnPlayerJoinEvent(PlayerJoinEvent obj)
    {
        if (PhotonNetwork.IsMasterClient)
            SendSyncData((obj.Player as PhotonPlayer).NetworkClient);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        photonView.RPC(nameof(SendPlateSyncDataRPC), player, gameObject.activeSelf);
    }

    [PunRPC]
    private void SendPlateSyncDataRPC(bool isActive)
    {
        SetActive(isActive);
    }

    public void OnDeliver()
    {
        photonView.RPC(nameof(OnDeliverRPC), RpcTarget.All);
    }

    [PunRPC]
    private void OnDeliverRPC()
    {
        gameObject.SetActive(false);
        FoodStack.RemoveAllIngredients(true);
    }

    public bool CanBeDelivered()
    {
        return (foodStack != null && foodStack.IngredientsStack.Count >= DISH_MIN_INGREDIENTS);
    }

    public void SetActive(bool toggle)
    {
        photonView.RPC(nameof(SetActiveRPC), RpcTarget.All, toggle);
    }

    [PunRPC]
    private void SetActiveRPC(bool toggle)
    {
        gameObject.SetActive(toggle);
    }
}
