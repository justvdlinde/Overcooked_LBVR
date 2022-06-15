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

    public void OnDeliver()
    {
        Debug.Log("Ondeliver");
        photonView.RPC(nameof(OnDeliverRPC), RpcTarget.All);
    }

    [PunRPC]
    private void OnDeliverRPC()
    {
        Debug.Log("OnDeliveRPC");

        // TODO: instantiate some kind of particle/feedback
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    public bool CanBeDelivered()
    {
        return (foodStack != null && foodStack.IngredientsStack.Count >= DeliveryPoint.DISH_MIN_INGREDIENTS);
    }
}
