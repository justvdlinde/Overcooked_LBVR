using Photon.Pun;
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

        // TODO: initial SetActivev sync for players joining midgame
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
