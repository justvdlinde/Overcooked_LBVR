using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryModeScoreboard : MonoBehaviourPunCallbacks, IGameModeScoreboard
{
    /// <summary>
    /// Orders that have been finished, either delivered of time exceeded
    /// </summary>
    public int FinishedOrdersCount { get; private set; }
    public float TotalPoints { get; private set; }
    public float MaxAchievablePoints { get; private set; }
    public int DeliveredOrdersCount { get; private set; }
    public int TimerExceededOrdersCount { get; private set; }

    public Action<OrderScoreData> EntryAddedEvent;

    // Is NOT synced at the moment
    private List<OrderScoreData> OrderScoreHistory;

    public void Awake()
    {
        OrderScoreHistory = new List<OrderScoreData>();
    }

    public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer newPlayer)
    {
        //if (PhotonNetwork.IsMasterClient)
        //    SendSyncData(newPlayer);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        Debug.Log("send sync data, finishedOrders " + FinishedOrdersCount);
        photonView.RPC(nameof(SendSyncDataRPC), player, FinishedOrdersCount, TotalPoints, MaxAchievablePoints , DeliveredOrdersCount, TimerExceededOrdersCount);
    }

    // could potentially be expanded to also send the OrderScoreData
    [PunRPC]
    private void SendSyncDataRPC(int finishedOrders, float totalPoints, float maxPoints, int deliveredOrders, int timerExceededOrders)
    {
        Debug.Log("sync received, orders: " + finishedOrders);
        FinishedOrdersCount = finishedOrders;
        TotalPoints = totalPoints;
        MaxAchievablePoints = maxPoints;
        DeliveredOrdersCount = deliveredOrders;
        TimerExceededOrdersCount = timerExceededOrders;
    }

    public void AddScore(Order order, OrderScore score)
    {
        AddScore(new OrderScoreData(order, score));
    }

    public void AddScore(OrderScoreData data)
    {
        photonView.RPC(nameof(AddScoreRPC), RpcTarget.Others, data.Score.Points, data.Score.MaxPoints, data.Score.Result);
        OrderScoreHistory.Add(data);
        EntryAddedEvent?.Invoke(data);
        AddScoreInternal(data.Score.Points, data.Score.MaxPoints, data.Score.Result);
    }

    [PunRPC]
    private void AddScoreRPC(float points, float maxPoints, DishResult result)
    {
        AddScoreInternal(points, maxPoints, result);
    }

    private void AddScoreInternal(float points, float maxPoints, DishResult result)
    {
        MaxAchievablePoints += maxPoints;
        TotalPoints += points;
        FinishedOrdersCount++;

        if (result == DishResult.Delivered)
            DeliveredOrdersCount++;
        else if (result == DishResult.TimerExceeded)
            TimerExceededOrdersCount++;
    }

    public override string ToString()
    {
        return TotalPoints + "/" + MaxAchievablePoints;
    }

    // TODO: remove? and simply create a new instance
    public void Reset()
    {
        OrderScoreHistory = new List<OrderScoreData>();
        TotalPoints = 0;
        FinishedOrdersCount = 0;
        MaxAchievablePoints = 0;
        DeliveredOrdersCount = 0;
        TimerExceededOrdersCount = 0;
    }

    public void Dispose() { }
}
