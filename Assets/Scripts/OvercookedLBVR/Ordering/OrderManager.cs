using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Extensions;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviourPun
{
    public static OrderManager Instance;

    public List<Order> Orders { get; private set; } = new List<Order>();
    public Action<Order> OnOrderAdded;
    public Action<Order> OnOrderRemoved;

    [SerializeField] private OrderDisplayManager displayManager;
    [SerializeField] private int orderAmount = 10;
    [SerializeField] private float[] orderDelays;

    [Header("Order Settings")]
    [SerializeField] private int minIngredients = 1;
    [SerializeField] private int maxIngredients = 8;
    [SerializeField] private float minTime = 20;
    [SerializeField] private float maxTime = 50;

    private int orderIndex;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple OrderManager instances found, destroying this instance");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(OrderCoroutine());
    }

    private System.Collections.IEnumerator OrderCoroutine()
    {
        for (int i = 0; i < orderDelays.Length; i++)
        {
            yield return new WaitForSeconds(orderDelays[orderIndex]);
            while (!CanAddNewOrder())
            {
                yield return null;
            }
            AddNewOrder();
        }
    }

    private bool CanAddNewOrder()
    {
        return displayManager.HasFreeDisplay();
    }

    [Button]
    private void AddNewOrder()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Order order = GenerateRandomOrder(minIngredients, maxIngredients);
            float timerDuration = Random.Range(minTime, maxTime);
            int[] ingredientInts = new int[order.ingredients.Length];
            for (int i = 0; i < ingredientInts.Length; i++)
            {
                ingredientInts[i] = (int)order.ingredients[i];
            }

            photonView.RPC(nameof(CreateNewOrderRPC), RpcTarget.All, ingredientInts, timerDuration);
        }
    }

    [PunRPC]
    private void CreateNewOrderRPC(int[] ingredients, float timerDuration, PhotonMessageInfo info)
    {
        Order order = new Order();
        order.ingredients = new IngredientType[ingredients.Length];

        for (int i = 0; i < ingredients.Length; i++)
        {
            order.ingredients[i] = (IngredientType)ingredients[i];
        }
        order.timer.Set(timerDuration);

        Orders.Add(order);
        OnOrderAdded?.Invoke(order);
        order.timer.Start();
        orderIndex++;
    }

    public void OnOrderTimerExceeded(Order order)
    {
        RemoveOrder(order);
    }

    public void OnOrderDelivered(Order order)
    {
        RemoveOrder(order);
    }

    private void RemoveOrder(Order order)
    {
        OnOrderRemoved?.Invoke(order);
        Orders.Remove(order);
        order.Dispose();
    }

    private Order GenerateRandomOrder(int minIngredients, int maxIngredients)
    {
        Order order = new Order();
        order.ingredients = new IngredientType[Random.Range(minIngredients + 2, maxIngredients + 2)];
        order.ingredients[0] = IngredientType.BunBottom;
        order.ingredients[order.ingredients.Length - 1] = IngredientType.BunTop;

        List<IngredientType> availableIngredients = Enum.GetValues(typeof(IngredientType)).Cast<IngredientType>().ToList();
        availableIngredients.Remove(IngredientType.None);
        availableIngredients.Remove(IngredientType.BunBottom);
        availableIngredients.Remove(IngredientType.BunTop);

        bool includesPatty = false;
        for (int i = 1; i < order.ingredients.Length - 1; i++)
        {
            order.ingredients[i] = availableIngredients.GetRandom();
            if (order.ingredients[i] == IngredientType.Patty)
                includesPatty = true;
        }

        if (!includesPatty)
            order.ingredients[Random.Range(1, order.ingredients.Length - 2)] = IngredientType.Patty;

        return order;
    }
}
