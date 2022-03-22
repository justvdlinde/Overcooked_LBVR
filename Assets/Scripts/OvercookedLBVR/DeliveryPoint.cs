using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviourPun
{
    public List<Dish> dishesInTrigger = new List<Dish>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Dish dish))
        {
            if (!dishesInTrigger.Contains(dish))
            {
                Debug.Log("OnTriggerEnter " + other.gameObject.name);
                dishesInTrigger.Add(dish);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Dish dish))
        {
            if (dishesInTrigger.Contains(dish))
            {
                Debug.Log("OnTriggerExit " + other.gameObject.name);
                dishesInTrigger.Remove(dish);
            }
        }
    }

    [Button]
    public void DeliverDishesInTrigger()
    {
        photonView.RPC(nameof(DeliverDishesInTriggerRPC), RpcTarget.All);
    }

    [PunRPC]
    private void DeliverDishesInTriggerRPC(PhotonMessageInfo info)
    {
		Action remove = null;
        foreach (Dish dish in dishesInTrigger)
		{
            if (dish != null)
            {
                bool delivered = DeliverDish(dish);
                if (delivered)
                {
                    remove += () => dishesInTrigger.Remove(dish);
                }
            }
		}

		remove?.Invoke();
    }

    [Button]
    public void DeliverDishDebug()
    {
        Dish dish = new GameObject().AddComponent<Dish>();
        DeliverDish(dish);
    }

    public bool DeliverDish(Dish dish)
    {
		bool delivered = false;
        Order closestOrder = OrderManager.Instance.GetClosestOrder(dish);
        if (closestOrder != null)
		{
            OrderManager.Instance.DeliverOrder(closestOrder, dish);
			delivered = true;
		}
        else
            Debug.LogError("No closest order found!");

        if (dish.transform.parent != null)
        {
            if(dish.transform.parent.parent != null)
            { 
                Destroy(dish.transform.parent.parent.gameObject);
            }
            else
            {
                Destroy(dish.transform.parent.gameObject);
            }

        }
        else
        {
            Destroy(dish.transform.gameObject);
        }
		return delivered;
    }
}
