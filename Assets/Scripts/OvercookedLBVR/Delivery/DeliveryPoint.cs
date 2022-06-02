using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Services;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviour
{
    public const int DISH_MIN_INGREDIENTS = 3;

    private List<Plate> platesInTrigger = new List<Plate>();
    private GameModeService gamemodeService;

    private void Awake()
    {
        gamemodeService = GlobalServiceLocator.Instance.Get<GameModeService>();
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

        for (int i = 0; i < platesInTrigger.Count; i++)
        {
            Plate plate = platesInTrigger[i];
            if (plate == null)
            {
                platesInTrigger.Remove(plate);
                continue;
            }

            Debug.Log("plate.CanBeDelivered " + plate.CanBeDelivered());
            if (plate.CanBeDelivered())
                DeliverDish(plate);
        }
    }

    public void DeliverDish(Plate dish)
    {
        if(gamemodeService.CurrentGameMode != null)
            gamemodeService.CurrentGameMode.DeliverDish(dish);

        platesInTrigger.Remove(dish);
        dish.OnDeliver();
    }
}
