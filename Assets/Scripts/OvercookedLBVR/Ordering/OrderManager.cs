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

    public List<Order> ActiveOrders { get; private set; } = new List<Order>();
    public Action<Order> OrderAddedToGame;
    public Action<Order> OrderFailed;
    public Action<Order, Dish> OrderDelivered;
    public int OrdersLeft => orderDelays.Length - currentOrderIndex;

    [SerializeField] private OrderDisplayManager displayManager;
    [SerializeField] private int orderAmount = 10;
    [SerializeField] private float[] orderDelays;
    [SerializeField] private bool useOrderTimers = true;

    [Header("Order Settings")]
    [SerializeField] private int minIngredients = 1;
    [SerializeField] private int maxIngredients = 3;
    [SerializeField] private float minTime = 20;
    [SerializeField] private float maxTime = 50;

    public int currentOrderIndex;

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
    }

    public void StartOrders()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(OrderCoroutine());
    }

    private System.Collections.IEnumerator OrderCoroutine()
    {
        for (int i = 0; i < orderDelays.Length; i++)
        {
            yield return new WaitForSeconds(orderDelays[currentOrderIndex]);
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

            photonView.RPC(nameof(CreateNewOrderRPC), RpcTarget.All, ingredientInts, timerDuration, useOrderTimers);
        }
    }

    [PunRPC]
    private void CreateNewOrderRPC(int[] ingredients, float timerDuration, bool useTimer, PhotonMessageInfo info)
    {
        Order order = new Order();
        order.ingredients = new IngredientType[ingredients.Length];

        for (int i = 0; i < ingredients.Length; i++)
        {
            order.ingredients[i] = (IngredientType)ingredients[i];
        }
        order.timer.Set(timerDuration);

        ActiveOrders.Add(order);
        OrderAddedToGame?.Invoke(order);
        if(useTimer)
            order.timer.Start();
        currentOrderIndex++;
    }

    public void OnOrderTimerExceeded(Order order)
    {
        ActiveOrders.Remove(order);
        OrderFailed?.Invoke(order);
        order.Dispose();
    }

    public void DeliverOrder(Order order, Dish dish)
    {
        ActiveOrders.Remove(order);
        OrderDelivered?.Invoke(order, dish);
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

    public Order GetClosestOrder(Dish dish)
    {
        // compare dish to each active order,
        // give score to each and return highest score
        if (ActiveOrders == null || ActiveOrders.Count == 0)
            return null;

        Order bestFit = ActiveOrders[0];
        int bestFitScore = 0;

        foreach(Order order in ActiveOrders)
        {
            IngredientType[] dishIngredients = dish.ingredients.Select(i => i.ingredientType).ToArray();
            IEnumerable<IngredientType> intersect = dishIngredients.Intersect(order.ingredients);

            int length = intersect.Count();
            if (length > bestFitScore)
            {
                bestFitScore = length;
                bestFit = order;
            }
        }

        if (bestFit == null)
            return ActiveOrders[0];
        else
            return bestFit;
    } 

    [Header("Debug")]
    public Dish testDish;

    [Button]
    private void CompareDishOrders()
    {
        Order best = GetClosestOrder(testDish);
        foreach (IngredientType ing in best.ingredients)
            Debug.Log(ing);
    }
}
