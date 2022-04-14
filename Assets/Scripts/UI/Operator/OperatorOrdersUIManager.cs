using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperatorOrdersUIManager : MonoBehaviour
{
    [SerializeField] private OrderDisplay[] displays;

    private Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();

    //private IEnumerator Start()
    //{
    //    OrdersController.Instance.OrderAddedToGame += DisplayOrder;
    //    OrdersController.Instance.OrderFailed += RemoveDisplay;
    //    OrdersController.Instance.OrderDelivered += OnOrderDelivered;
    //}

    //private void OnDestroy()
    //{
    //    OrdersController.Instance.OrderAddedToGame -= DisplayOrder;
    //    OrdersController.Instance.OrderFailed -= RemoveDisplay;
    //    OrdersController.Instance.OrderDelivered -= OnOrderDelivered;
    //}

    private void DisplayOrder(Order order)
    {
        OrderDisplay display = displays[order.orderNumber];
        display.Show(order);
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

    private void OnOrderDelivered(Order order, FoodStack dish)
    {
        RemoveDisplay(order);
    }
}
