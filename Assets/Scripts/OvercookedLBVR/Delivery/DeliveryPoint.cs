using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviourPun
{
    public List<Dish> dishesInTrigger = new List<Dish>();

    private GlobalEventDispatcher globalEventDispatcher;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out DishTrigger trigger))
        {
            if (!dishesInTrigger.Contains(trigger.dish))
            {
                dishesInTrigger.Add(trigger.dish);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out DishTrigger trigger))
        {
            if (dishesInTrigger.Contains(trigger.dish))
            {
                dishesInTrigger.Remove(trigger.dish);
            }
        }
    }

    [Button]
    public void DeliverDishesInTrigger()
    {
		Action remove = null;
        foreach (Dish dish in dishesInTrigger)
		{
            if (dish != null && dish.ingredients != null && dish.ingredients.Count > 0)
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
        Order closestOrder = OrdersController.Instance.GetClosestOrder(dish, out float bestFitScore);
        Debug.Log("Delivered dish: " + string.Join("", dish.ingredients)
            + "\nbest fit score: " + bestFitScore
            + "\nclosest order: " + string.Join("", closestOrder.ingredients));

        if (closestOrder != null)
        {
            OrdersController.Instance.DeliverOrder(closestOrder, dish);
            delivered = true;
        }
        else
            Debug.LogError("No closest order found!");

        if (dish.transform.parent != null)
        {
            if (dish.transform.parent.parent != null)
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
