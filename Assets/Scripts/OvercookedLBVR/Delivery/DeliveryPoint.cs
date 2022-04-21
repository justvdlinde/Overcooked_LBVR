using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviour
{
    public const int DISH_MIN_INGREDIENTS = 3;

    private List<Plate> platesInTrigger = new List<Plate>();
    private GlobalEventDispatcher globalEventDispatcher;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Plate plate))
        {
            if (!platesInTrigger.Contains(plate))
            {
                platesInTrigger.Add(plate);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Plate plate))
        {
            if (platesInTrigger.Contains(plate))
            {
                platesInTrigger.Remove(plate);
            }
        }
    }

    [Button]
    public void DeliverDishesInTrigger()
    {
        Debug.Log("platesInTrigger.Count: " + platesInTrigger.Count);
        foreach (Plate plate in platesInTrigger)
		{
            Debug.Log("plate.CanBeDelivered " + plate.CanBeDelivered());
            if (plate.CanBeDelivered())
            {
                DeliverDish(plate);
            }
		}
    }

    public void DeliverDish(Plate dish)
    {
        globalEventDispatcher.Invoke(new DishDeliveredEvent(dish, this));
    }
}
