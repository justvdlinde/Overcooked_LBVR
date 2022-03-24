using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperatorOrdersUIManager : MonoBehaviour
{
    [SerializeField] private OrderDisplay[] displays;

    private Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();

    private IEnumerator Start()
    {
        while(OrderManager.Instance == null)
        {
            yield return new WaitForSeconds(1);
        }

        OrderManager.Instance.OrderAddedToGame += DisplayOrder;
        OrderManager.Instance.OrderFailed += RemoveDisplay;
        OrderManager.Instance.OrderDelivered += OnOrderDelivered;
    }

    private void OnDestroy()
    {
        OrderManager.Instance.OrderAddedToGame -= DisplayOrder;
        OrderManager.Instance.OrderFailed -= RemoveDisplay;
        OrderManager.Instance.OrderDelivered -= OnOrderDelivered;
    }

    private void DisplayOrder(Order order)
    {
        OrderDisplay display = displays[order.orderNumber];
        display.DisplayOrder(order);
        orderDisplayPairs.Add(order, display);
    }

    public void RemoveDisplay(Order order)
    {
        if (orderDisplayPairs.TryGetValue(order, out OrderDisplay display))
        {
            display.Clear();
            orderDisplayPairs.Remove(order);
        }
    }

    private void OnOrderDelivered(Order order, Dish dish)
    {
        RemoveDisplay(order);
    }
}
