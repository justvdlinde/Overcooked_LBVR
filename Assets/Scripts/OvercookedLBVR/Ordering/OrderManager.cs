using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Extensions;
using Random = UnityEngine.Random;

public class OrderManager : MonoBehaviour
{
    public List<Order> Orders { get; private set; } = new List<Order>();
    public Action<Order> OnOrderAdded;
    public Action<Order, Score> OnOrderFinished;

    [SerializeField] private int minIngredients = 1;
    [SerializeField] private int maxIngredients = 8;
    [SerializeField] private float minTime = 20;
    [SerializeField] private float maxTime = 50;

    private void Start()
    {
        AddNewOrder();
    }

    public void PrintScore()
    {
        //Debug.Log(new Score(order, testDish));
    }

    [Button]
    private void AddNewOrder()
    {
        Order order = CreateNewOrder();
        Orders.Add(order);
        OnOrderAdded?.Invoke(order);
        order.timer.Start();
    }

    public Order CreateNewOrder()
    {
        Order order = GenerateRandomOrder(minIngredients, maxIngredients);
        Timer timer = new Timer();
        timer.Set(Random.Range(minTime, maxTime));
        order.timer = timer;
        return order;
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

        for (int i = 1; i < order.ingredients.Length - 1; i++)
        {
            order.ingredients[i] = availableIngredients.GetRandom();
        }

        return order;
    }
}
