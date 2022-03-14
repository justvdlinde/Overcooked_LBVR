using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Attributes;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private Order order = null;
    [SerializeField] private Dish testDish = null;

    [Button]
    public void PrintScore()
    {
        Debug.Log(new Score(order, testDish));
    }
}
