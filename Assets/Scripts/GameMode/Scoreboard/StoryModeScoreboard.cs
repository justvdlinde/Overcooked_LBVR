using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;

public class StoryModeScoreboard : MonoBehaviourPunCallbacks, IGameModeScoreboard
{
    /// <summary>
    /// Orders that have been finished, either delivered or time exceeded
    /// </summary>
    public int OrdersCount { get; private set; }
    public float TotalPoints { get; private set; }
    public float MaxAchievablePoints { get; private set; }
    public int DeliveredOrdersCount { get; private set; }
    public int TimerExceededOrdersCount { get; private set; }
    public int PerfectOrdersCount { get; private set; }

    public Action<OrderScorePair> EntryAddedEvent;

    // Is NOT synced at the moment
    private List<OrderScorePair> OrderScoreHistory;

    public void Awake()
    {
        OrderScoreHistory = new List<OrderScorePair>();
    }

    public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            SendSyncData(newPlayer);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        photonView.RPC(nameof(SendSyncDataRPC), player, OrdersCount, TotalPoints, MaxAchievablePoints , DeliveredOrdersCount, TimerExceededOrdersCount, PerfectOrdersCount);
    }

    // could potentially be expanded to also send the OrderScoreData
    [PunRPC]
    private void SendSyncDataRPC(int finishedOrders, float totalPoints, float maxPoints, int deliveredOrders, int timerExceededOrders, int perfectOrdersCount)
    {
        OrdersCount = finishedOrders;
        TotalPoints = totalPoints;
        MaxAchievablePoints = maxPoints;
        DeliveredOrdersCount = deliveredOrders;
        TimerExceededOrdersCount = timerExceededOrders;
        PerfectOrdersCount = perfectOrdersCount;
    }

    public void AddScore(Order order, ScoreData score)
    {
        AddScore(new OrderScorePair(order, score));
    }

    public void AddScore(OrderScorePair data)
    {
        photonView.RPC(nameof(AddScoreRPC), RpcTarget.Others, data.Score.Points, ScoreData.MaxPoints, data.Score.Result);
        OrderScoreHistory.Add(data);
        EntryAddedEvent?.Invoke(data);
        AddScoreInternal(data.Score.Points, ScoreData.MaxPoints, data.Score.Result);
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
        OrdersCount++;

        if (points == maxPoints)
            PerfectOrdersCount++;

        if (result == DishResult.Delivered)
            DeliveredOrdersCount++;
        else if (result == DishResult.TimerExceeded)
            TimerExceededOrdersCount++;
    }

    public override string ToString()
    {
        return TotalPoints + "/" + MaxAchievablePoints;
    }

    public void Reset()
    {
        OrderScoreHistory = new List<OrderScorePair>();
        TotalPoints = 0;
        OrdersCount = 0;
        MaxAchievablePoints = 0;
        DeliveredOrdersCount = 0;
        TimerExceededOrdersCount = 0;
    }

    public void Dispose() { }
}
