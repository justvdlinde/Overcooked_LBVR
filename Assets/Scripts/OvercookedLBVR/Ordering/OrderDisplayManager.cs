using System.Collections.Generic;
using UnityEngine;

public class OrderDisplayManager : MonoBehaviour
{
    [SerializeField] private OrderManager orderManager = null;
    [SerializeField] private OrderDisplay[] displays;

    private Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();

    private void OnEnable()
    {
        orderManager.OnOrderAdded += SetupDisplay;
        orderManager.OnOrderFinished += OnOrderFinished;
    }

    private void OnDisable()
    {
        orderManager.OnOrderAdded -= SetupDisplay;
        orderManager.OnOrderFinished -= OnOrderFinished;
    }

    private void SetupDisplay(Order order)
    {
        OrderDisplay display = GetBestEmptyDisplay();
        display.DisplayOrder(order);
        orderDisplayPairs.Add(order, display);
    }

    private OrderDisplay GetBestEmptyDisplay()
    {
        foreach(OrderDisplay display in displays)
        {
            if (display.Order == null)
                return display;
        }

        return null;
    }

    private void OnOrderFinished(Order order, Score score)
    {
        RemoveDisplay(order);
    }

    public void RemoveDisplay(Order order)
    {
        if(orderDisplayPairs.TryGetValue(order, out OrderDisplay display))
        {
            Destroy(display.gameObject);
            orderDisplayPairs.Remove(order);
        }
    }
}
