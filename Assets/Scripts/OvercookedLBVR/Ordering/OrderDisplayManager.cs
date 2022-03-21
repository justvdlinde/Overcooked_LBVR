using System.Collections.Generic;
using UnityEngine;

public class OrderDisplayManager : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager = null;
    [SerializeField] private OrderDisplay[] displays;

    private Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();

    private void OnEnable()
    {
        orderManager.OrderAddedToGame += DisplayOrder;
        orderManager.OrderFailed += RemoveDisplay;
        orderManager.OrderDelivered += OnOrderDelivered;
    }

    private void OnDisable()
    {
        orderManager.OrderAddedToGame -= DisplayOrder;
        orderManager.OrderFailed -= RemoveDisplay;
        orderManager.OrderDelivered -= OnOrderDelivered;
    }

    public bool HasFreeDisplay()
    {
        return HasFreeDisplay(out OrderDisplay display);
    }

    public bool HasFreeDisplay(out OrderDisplay display)
    {
        display = null;
        foreach (OrderDisplay d in displays)
        {
            if (d.Order == null)
            {
                display = d;
                return true;
            }
        }
        return false;
    }

    private void DisplayOrder(Order order)
    {
        if (HasFreeDisplay(out OrderDisplay display))
        {
            display.DisplayOrder(order);
            orderDisplayPairs.Add(order, display);
        }
        else
        {
            Debug.LogWarning("No free display found");
        }
    }

    public void RemoveDisplay(Order order)
    {
        if(orderDisplayPairs.TryGetValue(order, out OrderDisplay display))
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
