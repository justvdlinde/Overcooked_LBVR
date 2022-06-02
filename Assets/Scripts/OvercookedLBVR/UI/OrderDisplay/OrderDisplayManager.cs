using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class OrderDisplayManager : MonoBehaviourPun
{
    public OrderDisplay[] OrderDisplays => orderDisplays;
    [SerializeField] protected OrderDisplay[] orderDisplays = null;

    protected Dictionary<Order, OrderDisplay> orderDisplayPairs = new Dictionary<Order, OrderDisplay>();
    protected GlobalEventDispatcher globalEventDispatcher;

    protected virtual void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        for(int i = 0; i < orderDisplays.Length; i++)
        {
            OrderDisplays[i].orderNumber = i;
        }
    }

    public void Start()
    {
        if(!PhotonNetwork.IsMasterClient)
            SyncDisplays();
    }

    private void SyncDisplays()
    {
        GameModeService gameModeService = GlobalServiceLocator.Instance.Get<GameModeService>();
        if (gameModeService.CurrentGameMode != null)
        {
            OrdersController ordersController = gameModeService.CurrentGameMode.OrdersController;
            if (ordersController != null && ordersController.ActiveOrders.Count > 0)
            {
                for (int i = 0; i < ordersController.ActiveOrders.Count; i++)
                {
                    Order order = ordersController.ActiveOrders[i];
                    DisplayOrder(order);
                }
            }
        }
    }

    public void OnEnable()
    {
        globalEventDispatcher.Subscribe<ActiveOrderAddedEvent>(OnNewActiveOrderEvent);
        globalEventDispatcher.Subscribe<ActiveOrderRemovedEvent>(OnActiveOrderRemovedEvent);
    }

    public void OnDisable()
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
    protected void AutoFindDisplays()
    {
        OrderDisplay[] displays = FindObjectsOfType<OrderDisplay>();
        orderDisplays = displays.OrderBy(item => item.orderNumber).ToArray();
    }
#endif
}
