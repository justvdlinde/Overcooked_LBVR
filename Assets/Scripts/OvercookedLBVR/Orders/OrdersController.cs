using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OrdersController : MonoBehaviourPunCallbacks
{
    public List<Order> ActiveOrders { get; private set; } = new List<Order>();
    public List<Order> CompletedOrders { get; private set; } = new List<Order>();

    // TODO: replace/add IEvents
    public Action<Order> ActiveOrderAdded;
    public Action<Order> ActiveOrderRemoved;

    // try to merge these 2 together?
    public Action<Order> OrderFailed;
    public Action<Order, Dish> OrderDelivered;

    public int CurrentOrderIndex { get; private set; }
    public int OrdersLeft => settings.orderAmount - CurrentOrderIndex;

    private TieredOrderGenerator orderGenerator;
    private GameSettings settings;

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
            RequestInitialSync();
    }

    public void RequestInitialSync()
    {
        photonView.RPC(nameof(RequestInitialSyncRPC), PhotonNetwork.MasterClient);
    }

    [PunRPC]
    private void RequestInitialSyncRPC(PhotonMessageInfo info)
    {
        Debug.Log("RequestDataRPC");
        SubmitInitialSyncData(info.Sender);
    }

    private void SubmitInitialSyncData(PhotonNetworkedPlayer player)
    {
        photonView.RPC(nameof(SubmitInitialSyncDataRPC), player);
    }

    [PunRPC]
    private void SubmitInitialSyncDataRPC()
    {
        Debug.Log("SendData received");
    }

    public void Initialize(GameSettings settings)
    {
        orderGenerator = new TieredOrderGenerator(); // add as 'generic' parameter
        this.settings = settings;
        CurrentOrderIndex = 0;

        // if joining late, sync up orders
        //if (PhotonNetwork.IsMasterClient)
        //{
        //}
    }

    public bool CanCreateNewOrder()
    {
        OrderDisplayManager displays = OrderDisplayManager.Instance;
        if (displays == null)
        {
            Debug.LogWarning("No Order Displays found!, make sure there is at least one present in scene.");
            return false;
        }
        else
        {
            return displays.HasFreeDisplay();
        }
    }

    public void CreateNewActiveOrder()
    {
        Order order = orderGenerator.GenerateRandomOrder(3, 0, out int newTier, true);
        order.timer.Set(4);
        AddActiveOrder(order);
    }

    public void AddActiveOrder(Order order)
    {
        photonView.RPC(nameof(AddActiveOrderRPC), RpcTarget.All, OrderSerializer.Serialize(order));
    }

    [PunRPC]
    private void AddActiveOrderRPC(byte[] orderData)
    {
        Order order = OrderSerializer.Deserialize(orderData);

        ActiveOrders.Add(order);
        ActiveOrderAdded?.Invoke(order);
        order.timer.Start();
        CurrentOrderIndex++;
        order.TimerExceededEvent += OnOrderTimerExceeded;

        // TODO: do this via events
        OrderDisplayManager.Instance.DisplayOrder(order);
    }

    public void OnOrderTimerExceeded(Order order)
    {
        // TODO: do this via events
        OrderDisplayManager.Instance.RemoveDisplay(order);

        OrderFailed?.Invoke(order);
        RemoveOrder(order);
    }

    private void RemoveOrder(Order order)
    {
        ActiveOrders.Remove(order);
        CompletedOrders.Add(order);
        ActiveOrderRemoved?.Invoke(order);

        order.TimerExceededEvent -= OnOrderTimerExceeded;
        order.Dispose();
    }

    public void DeliverOrder(Order order, Dish dish)
    {
        OrderDelivered?.Invoke(order, dish);
        RemoveOrder(order);
    }

    public Order GetClosestOrder(Dish dish, out float bestFitScore)
    {
        bestFitScore = 0;
        if (ActiveOrders == null || ActiveOrders.Count == 0)
            return null;

        Order bestFit = ActiveOrders[0];
        bestFitScore = GetDishScore(dish, ActiveOrders[0]);

        for (int i = 1; i < ActiveOrders.Count; i++)
        {
            Order order = ActiveOrders[i];
            float score = GetDishScore(dish, order);

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

    private float GetDishScore(Dish dish, Order order)
    {
        Score score = new Score(order, dish);
        return score.Points / score.MaxScore;
    }
}
