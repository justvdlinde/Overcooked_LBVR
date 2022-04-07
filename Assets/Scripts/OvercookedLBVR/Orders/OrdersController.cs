using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class OrdersController : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Needs a singleton because it needs to be instantiated by Photon by the MasterClient but all clients need a reference as well
    /// </summary>
    public static OrdersController Instance;

    public List<Order> ActiveOrders { get; private set; } = new List<Order>();
    public List<Order> CompletedOrders { get; private set; } = new List<Order>();
    public int CurrentOrderIndex { get; private set; }

    public Action<Order> ActiveOrderAdded;
    public Action<Order> ActiveOrderRemoved;

    private GlobalEventDispatcher globalEventDispatcher;
    private TieredOrderGenerator orderGenerator;

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        if (Instance != null)
        {
            Debug.LogWarning("OrdersController Instance already present, destroying old instance");
            PhotonNetwork.Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            SendSyncData(newPlayer);
    }

    [Utils.Core.Attributes.Button]
    private void RequestInitialSync()
    {
        photonView.RPC(nameof(RequestInitialSyncRPC), PhotonNetwork.MasterClient);
    }

    [PunRPC]
    private void RequestInitialSyncRPC(PhotonMessageInfo info)
    {
        SendSyncData(info.Sender);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        List<byte[]> bytes = new List<byte[]>();
        for (int i = 0; i < ActiveOrders.Count; i++)
        {
            byte[] order = OrderSerializer.Serialize(ActiveOrders[i]);
            bytes.Add(order);
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
            AddActiveOrder(order);
        }
    }

    public void Initialize(GameSettings settings) // TODO: add generator as generic parameter
    {
        orderGenerator = new TieredOrderGenerator(); 
        CurrentOrderIndex = 0;
    }

    public void CreateNewActiveOrder(int displayNr)
    {
        Order order = orderGenerator.GenerateRandomOrder(3, 0, out int newTier, true);
        order.orderNumber = displayNr;
        order.timer.Set(5); // TODO: set timer
        SubmitActiveOrder(order);
    }

    public void SubmitActiveOrder(Order order)
    {
        photonView.RPC(nameof(SubmitActiveOrderRPC), RpcTarget.Others, OrderSerializer.Serialize(order));
        AddActiveOrder(order);
    }

    [PunRPC]
    private void SubmitActiveOrderRPC(byte[] orderData)
    {
        AddActiveOrder(OrderSerializer.Deserialize(orderData));
    }

    private void AddActiveOrder(Order order)
    {
        Debug.Log("AddActiveOrder " + order);
        ActiveOrders.Add(order);
        ActiveOrderAdded?.Invoke(order);
        order.timer.Start();

        CurrentOrderIndex++;
        order.TimerExceededEvent += OnOrderTimerExceeded;

        globalEventDispatcher.Invoke(new ActiveOrderAddedEvent(order));
    }

    public void OnOrderTimerExceeded(Order order)
    {
        // rpc call to all players to remove that order?
        // calculate score via gamemode?
        // send/store score rpc?

        RemoveActiveOrder(order);
    }

    private void RemoveActiveOrder(Order order)
    {
        ActiveOrders.Remove(order);
        CompletedOrders.Add(order);
        ActiveOrderRemoved?.Invoke(order);
        globalEventDispatcher.Invoke(new ActiveOrderRemovedEvent(order));

        order.TimerExceededEvent -= OnOrderTimerExceeded;
        order.Dispose();
    }

    public void DeliverOrder(Order order, Dish dish)
    {
        RemoveActiveOrder(order);
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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
