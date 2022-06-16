using Photon.Pun;
using System;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

[SelectionBase]
public class Plate : MonoBehaviourPun
{
    public FoodStack FoodStack => foodStack;

    [SerializeField] private FoodStack foodStack = null;

    private GlobalEventDispatcher globalEventdispatcher = null;

    private void Awake()
    {
        globalEventdispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    public void OnDeliver()
    {
        photonView.RPC(nameof(OnDeliverRPC), RpcTarget.All);
    }

    [PunRPC]
    private void OnDeliverRPC()
    {
        // TODO: instantiate some kind of particle/feedback
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    public bool CanBeDelivered()
    {
        return (foodStack != null && foodStack.IngredientsStack.Count >= DeliveryPoint.DISH_MIN_INGREDIENTS);
    }

    private void OnDestroy()
    {
		globalEventdispatcher.Invoke(new PlateDestroyedEvent());
    }
}
