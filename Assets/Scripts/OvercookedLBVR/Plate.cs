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

    private GlobalEventDispatcher globalEventDispatcher = null;

	private void OnEnable()
	{
        if (globalEventDispatcher == null)
            globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        globalEventDispatcher.Subscribe<StartGameEvent>(OnStartGameEvent);
	}

	private void OnDisable()
	{
        globalEventDispatcher.Unsubscribe<StartGameEvent>(OnStartGameEvent);
    }

    private void OnStartGameEvent(StartGameEvent obj)
	{
		if(photonView.IsMine)
            PhotonNetwork.Destroy(photonView.transform.gameObject);
    }

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
