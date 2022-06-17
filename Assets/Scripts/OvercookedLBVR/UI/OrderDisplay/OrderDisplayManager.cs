using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class OrderDisplayManager : MonoBehaviourPun
{
    public DeliveryPoint[] DeliveryPoints => deliveryPoints;
    [SerializeField] protected DeliveryPoint[] deliveryPoints = null;

    protected Dictionary<Order, DeliveryPoint> orderDeliveryPairs = new Dictionary<Order, DeliveryPoint>();
    protected GlobalEventDispatcher globalEventDispatcher;

    protected virtual void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        for(int i = 0; i < deliveryPoints.Length; i++)
        {
            DeliveryPoints[i].SetOrderIndex(i);
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
            if (ordersController != null)
            {
                for (int i = 0; i < ordersController.ActiveOrders.Length; i++)
                {
                    Order order = ordersController.ActiveOrders[i];
                    if(order != null)
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
        ClearDeliveryPointDisplay(@event.Order);
    }

    public void DisplayOrder(Order order)
    {
        DeliveryPoint deliveryPoint = deliveryPoints[order.orderIndex];
        if (!deliveryPoint.Display.CanBeUsed())
            Debug.LogWarning("Display is already in use, displayed order will be overwritten");

        deliveryPoint.Display.Show(order);
        orderDeliveryPairs.Add(order, deliveryPoint);
    }

    public void ClearDeliveryPointDisplay(Order order)
    {
        if(orderDeliveryPairs.TryGetValue(order, out DeliveryPoint deliveryPoint))
        {
            deliveryPoint.Display.Clear();
            orderDeliveryPairs.Remove(order);
        }
        else
        {
            Debug.LogError("No matching deliverypoint found for order with number: " + order);
        }
    }

#if UNITY_EDITOR
    [Utils.Core.Attributes.Button]
    protected void AutoFindDisplays()
    {
        DeliveryPoint[] deliveryPoints = FindObjectsOfType<DeliveryPoint>();
        deliveryPoints = deliveryPoints.OrderBy(item => item.OrderIndex).ToArray();
    }
#endif
}
