using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviour
{
    public const int DISH_MIN_INGREDIENTS = 3;

    private List<Dish> dishesInTrigger = new List<Dish>();
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
        foreach (Dish dish in dishesInTrigger)
		{
            if (dish != null && dish.ingredientsStack.Count > DISH_MIN_INGREDIENTS)
            {
                DeliverDish(dish);
            }
		}
    }

    public void DeliverDish(Dish dish)
    {
        globalEventDispatcher.Invoke(new DishDeliveredEvent(dish, this));
    }
}
