using System.Collections.Generic;
using UnityEngine;

public class OrderDisplayManager : MonoBehaviour
{
    public static OrderDisplayManager Instance;

    [SerializeField] private OrderDisplay[] displays;

    private OrdersController orderManager = null;
    private Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();

    private void Awake()
    {
        Instance = this;
    }

    //private void OnEnable()
    //{
    //    orderManager.OrderAddedToGame += DisplayOrder;
    //    orderManager.OrderFailed += RemoveDisplay;
    //    orderManager.OrderDelivered += OnOrderDelivered;
    //}

    //private void OnDisable()
    //{
    //    orderManager.OrderAddedToGame -= DisplayOrder;
    //    orderManager.OrderFailed -= RemoveDisplay;
    //    orderManager.OrderDelivered -= OnOrderDelivered;
    //}

    public bool HasFreeDisplay()
    {
        return HasFreeDisplay(out _);
    }

    public bool HasFreeDisplay(out OrderDisplay display)
    {
        display = GetFreeDisplay();
        return display != null;
    }

    public OrderDisplay GetFreeDisplay()
    {
        foreach (OrderDisplay d in displays)
        {
            if (d.Order == null)
                return d;
        }
        return null;
    }

    public void DisplayOrder(Order order)
    {
        if (HasFreeDisplay(out OrderDisplay display))
        {
            display.Show(order);
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
