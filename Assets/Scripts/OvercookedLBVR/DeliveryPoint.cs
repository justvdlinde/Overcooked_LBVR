using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviourPun
{
    private List<Dish> dishesInTrigger = new List<Dish>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Dish dish))
        {
            if(!dishesInTrigger.Contains(dish))
                dishesInTrigger.Add(dish);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Dish dish))
        {
            if(dishesInTrigger.Contains(dish))
                dishesInTrigger.Remove(dish);
        }
    }

    [Button]
    public void DeliverDishesInTrigger()
    {
        Debug.Log("DELIVER");
        photonView.RPC(nameof(DeliverDishesInTriggerRPC), RpcTarget.All);
    }

    [PunRPC]
    private void DeliverDishesInTriggerRPC(PhotonMessageInfo info)
    {
        foreach (Dish dish in dishesInTrigger)
            DeliverDish(dish);
    }

    public void DeliverDish(Dish dish)
    {
        Order closestOrder = OrderManager.Instance.GetClosestOrder(dish);
        OrderManager.Instance.DeliverOrder(closestOrder, dish);
        Destroy(dish.gameObject);
    }
}
