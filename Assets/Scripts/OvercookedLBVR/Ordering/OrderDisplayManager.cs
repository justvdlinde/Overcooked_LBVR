using System.Collections.Generic;
using UnityEngine;

public class OrderDisplayManager : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager = null;
    [SerializeField] private OrderDisplay[] displays;

    private Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();

    private void OnEnable()
    {
        orderManager.OnOrderAdded += DisplayOrder;
        orderManager.OnOrderRemoved += RemoveDisplay;
    }

    private void OnDisable()
    {
        orderManager.OnOrderAdded -= DisplayOrder;
        orderManager.OnOrderRemoved -= RemoveDisplay;
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
}
