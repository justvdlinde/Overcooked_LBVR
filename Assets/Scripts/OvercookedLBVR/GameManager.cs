using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;

public class GameManager : MonoBehaviourPun
{
    public OrderManager orderManager;

    private Dictionary<Order, Score> orderScorePairs = new Dictionary<Order, Score>();
    private float totalScore;
    private float maxScore;

    private bool gameIsRunning;

    private void OnEnable()
    {
        orderManager.OrderFailed += OnOrderFailed;
        orderManager.OrderDelivered += OnOrderDelivered;
    }

    private void OnDisable()
    {
        orderManager.OrderFailed -= OnOrderFailed;
    }

    private void OnOrderFailed(Order order)
    {
        Score score = new Score(order);
        AddScore(score, order);
        order.score = score;
    }

    private void OnOrderDelivered(Order order, Dish dish)
    {
        Score score = new Score(order, dish);
        AddScore(score, order);
        order.score = score;
    }

    private void AddScore(Score score, Order order)
    {
        orderScorePairs.Add(order, score);
        if (orderManager.OrdersLeft <= 0 && orderManager.ActiveOrders.Count == 0)
            GameOver();
    }

    [Button]
    public void StartGame()
    {
        if(!gameIsRunning)
            photonView.RPC(nameof(StartGameRPC), RpcTarget.All);
    }

    [PunRPC]
    private void StartGameRPC(PhotonMessageInfo info)
    {
        orderScorePairs = new Dictionary<Order, Score>();
        Debug.Log("Starting Game");
        gameIsRunning = true;

        if (PhotonNetwork.IsMasterClient)
            orderManager.StartOrders();

        GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>().Invoke(new StartGameEvent());
    }

    private void GameOver()
    {
        foreach(var kvp in orderScorePairs)
        {
            totalScore += kvp.Value.Points;
            maxScore += kvp.Value.MaxScore;
        }

        gameIsRunning = false;
        Debug.Log("Game Over!");
        Debug.LogFormat("Final score: {0} out of {1}", totalScore, maxScore);
    }
}
