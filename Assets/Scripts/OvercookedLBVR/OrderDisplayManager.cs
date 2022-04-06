using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class OrderDisplayManager : MonoBehaviour
{
    private static OrderDisplayManager Instance;

    public OrderDisplay[] OrderDisplays => orderDisplays;
    [SerializeField] private OrderDisplay[] orderDisplays = null;

    private Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();
    private GlobalEventDispatcher globalEventDispatcher;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;

        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if(!PhotonNetwork.IsMasterClient)
            SyncDisplays();
    }

    private void SyncDisplays()
    {
        OrdersController ordersController = OrdersController.Instance;
        if (ordersController != null && ordersController.ActiveOrders.Count > 0)
        {
            for (int i = 0; i < ordersController.ActiveOrders.Count; i++)
            {
                Order order = ordersController.ActiveOrders[i];
                DisplayOrder(order);
            }
        }
    }

    private void OnEnable()
    {
        globalEventDispatcher.Subscribe<ActiveOrderAddedEvent>(OnNewActiveOrderEvent);
        globalEventDispatcher.Subscribe<ActiveOrderRemovedEvent>(OnActiveOrderRemovedEvent);
    }


    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<ActiveOrderAddedEvent>(OnNewActiveOrderEvent);
        globalEventDispatcher.Unsubscribe<ActiveOrderRemovedEvent>(OnActiveOrderRemovedEvent);
    }

    private void OnNewActiveOrderEvent(ActiveOrderAddedEvent @event)
    {
        DisplayOrder(@event.Order);
    }

    private void OnActiveOrderRemovedEvent(ActiveOrderRemovedEvent @event)
    {
        ClearDisplay(@event.Order);
    }

    public static bool HasFreeDisplay()
    {
        return HasFreeDisplay(out _);
    }

    public static bool HasFreeDisplay(out OrderDisplay display)
    {
        display = GetFreeDisplay();
        return display != null;
    }

    public static OrderDisplay GetFreeDisplay()
    {
        foreach (OrderDisplay display in Instance.orderDisplays)
        {
            if (display.CanBeUsed())
                return display;
        }
        return null;
    }

    public void DisplayOrder(Order order)
    {
        OrderDisplay display = orderDisplays[order.orderNumber];
        if (!display.CanBeUsed())
            Debug.LogWarning("Display is already in use, displayed order will be overwritten");

        display.Show(order);
        orderDisplayPairs.Add(order, display);
    }

    public void ClearDisplay(Order order)
    {
        if(orderDisplayPairs.TryGetValue(order, out OrderDisplay display))
        {
            display.Clear();
            orderDisplayPairs.Remove(order);
        }
    }

#if UNITY_EDITOR
    [Utils.Core.Attributes.Button]
    private void AutoFindDisplays()
    {
        OrderDisplay[] displays = FindObjectsOfType<OrderDisplay>();
        orderDisplays = displays.OrderBy(item => item.orderNumber).ToArray();
    }
#endif
}
