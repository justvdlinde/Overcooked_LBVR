using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using Utils.Core.Events;
using Utils.Core.Services;

public class OrdersController : MonoBehaviourPun
{
    public Order[] ActiveOrders { get; private set; }
    public List<Order> CompletedOrders { get; private set; } = new List<Order>();
    public int CurrentOrderIndex { get; private set; }

    public Action<Order> ActiveOrderAdded;
    public Action<Order> ActiveOrderRemoved;

    private GlobalEventDispatcher globalEventDispatcher;
    private readonly DishFitnessCalculator fitnessCalculator = new DishFitnessCalculator();

    protected virtual void Awake()
    {
        SetOrderCount(4);

        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
        globalEventDispatcher.Subscribe<PlayerJoinEvent>(OnPlayerJoinedEvent);
    }

    protected virtual void OnDestroy()
    {
        globalEventDispatcher.Unsubscribe<PlayerJoinEvent>(OnPlayerJoinedEvent);
    }

    private void SetOrderCount(int count)
    {
        ActiveOrders = new Order[count];
    }

    // MonobehaviourPunCallbacks.OnPlayerEnteredRoom is not always being called, therefore this event is needed
    private void OnPlayerJoinedEvent(PlayerJoinEvent @event)
    {
        if (PhotonNetwork.IsMasterClient)
            SendSyncData((@event.Player as PhotonPlayer).NetworkClient);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        List<byte[]> bytes = new List<byte[]>();
        for (int i = 0; i < ActiveOrders.Length; i++)
        {
            if (ActiveOrders[i] != null)
            {
                byte[] order = OrderSerializer.Serialize(ActiveOrders[i]);
                bytes.Add(order);
            }
        }

        // In case ActiveOrders is empty, send null, otherwise Photon will throw an error
        object data = null;
        if (bytes.Count != 0)
            data = bytes.ToArray();

        photonView.RPC(nameof(SendSyncDataRPC), player, data);
    }

    [PunRPC]
    private void SendSyncDataRPC(object data)
    {
        if (data == null)
            return;

        byte[][] bytes = (byte[][])data;
        for (int i = 0; i < bytes.Length; i++)
        {
            Order order = OrderSerializer.Deserialize(bytes[i]);
            AddActiveOrderInternal(order);
        }
    }

    public void AddActiveOrder(Order order)
    {
        photonView.RPC(nameof(SubmitActiveOrderRPC), RpcTarget.Others, OrderSerializer.Serialize(order));
        AddActiveOrderInternal(order);
    }

    [PunRPC]
    private void SubmitActiveOrderRPC(byte[] orderData)
    {
        AddActiveOrderInternal(OrderSerializer.Deserialize(orderData));
    }

    private void AddActiveOrderInternal(Order order)
    {
        ActiveOrders[order.orderIndex] = order;
        ActiveOrderAdded?.Invoke(order);
        order.timer.Start();
        CurrentOrderIndex++;
        globalEventDispatcher.Invoke(new ActiveOrderAddedEvent(order));
    }

    public void RemoveAllActiveOrders()
    {
        photonView.RPC(nameof(RemoveAllActiveOrdersRPC), RpcTarget.All);
    }

    [PunRPC]
    public void RemoveAllActiveOrdersRPC()
    {
        Order[] temp = ActiveOrders;
        for (int i = 0; i < temp.Length; i++)
        {
            if(temp[i] != null)
                RemoveActiveOrderInternal(temp[i], false);
        }
    }

    public void RemoveActiveOrder(Order order)
    {
        photonView.RPC(nameof(SubmitRemoveActiveOrderRPC), RpcTarget.Others, order.orderIndex);
        RemoveActiveOrderInternal(order);
    }

    [PunRPC]
    private void SubmitRemoveActiveOrderRPC(int activeOrderIndex)
    {
        RemoveActiveOrderInternal(ActiveOrders[activeOrderIndex]);
    }

    private void RemoveActiveOrderInternal(Order order, bool addToCompleted = true)
    {
        ActiveOrders[order.orderIndex] = null;
        if(addToCompleted)
            CompletedOrders.Add(order);
        ActiveOrderRemoved?.Invoke(order);
        globalEventDispatcher.Invoke(new ActiveOrderRemovedEvent(order));
        order.Dispose();
    }

    public Order GetOrder(int orderIndex)
    {
        if (orderIndex > ActiveOrders.Length)
            return null;
        else
            return ActiveOrders[orderIndex];
    }

    public int ActiveOrdersCount()
    {
        int count = 0;
        for (int i = 0; i < ActiveOrders.Length; i++)
        {
            if (ActiveOrders[i] != null)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Returns order that best matches dish
    /// </summary>
    /// <param name="dish"></param>
    /// <param name="bestFitScore"></param>
    /// <returns></returns>
    public Order GetClosestMatch(FoodStack dish)
    {
        if (ActiveOrders == null || ActiveOrders.Length == 0)
            return null;

        Order bestFit = ActiveOrders[0];
        float bestFitScore = fitnessCalculator.CalculateFitness(dish.Compare(ActiveOrders[0]));

        for (int i = 1; i < ActiveOrders.Length; i++)
        {
            Order order = ActiveOrders[i];
            float score = fitnessCalculator.CalculateFitness(dish.Compare(order));

            if (score > bestFitScore)
            {
                bestFit = order;
                bestFitScore = score;
            }
            else if (score == bestFitScore)
            {
                // choose the one with less time remaining
                if (order.timer.TimeRemaining < bestFit.timer.TimeRemaining)
                {
                    bestFit = order;
                    bestFitScore = score;
                }
            }
        }

        if (bestFit == null)
            return ActiveOrders[0];
        else
            return bestFit;
    }
}
