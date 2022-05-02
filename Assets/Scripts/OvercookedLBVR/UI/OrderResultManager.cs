using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class OrderResultManager : MonoBehaviour
{
    [SerializeField] private OrderResultUI resultPrefab = null;
    [SerializeField] private float yOffset = 0.4f;

    private GlobalEventDispatcher globalEventDispatcher;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnEnable()
    {
        globalEventDispatcher.Subscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
    }

    private void OnDishDeliveredEvent(DishDeliveredEvent @event)
    {
        Vector3 spawnPos;
        if (@event.Dish == null)
        {
            spawnPos = transform.position;
        }
        else
        {
            spawnPos = @event.Dish.gameObject.transform.position;
            spawnPos.y += yOffset;
        }

        OrderResultUI instance = Instantiate(resultPrefab, spawnPos, Quaternion.identity);
        instance.Setup(@event.Score);
    }
}
