using UnityEngine;
using Utils.Core.Attributes;

public class DishFitnessCalculatorTester : MonoBehaviour
{
    [SerializeField] private FoodStack foodStack = null;
    [SerializeField] private Order order = null;

    private DishFitnessCalculator calculator = new DishFitnessCalculator();

    [Button]
    private void PrintScore()
    {
        if (foodStack == null)
        {
            Debug.LogWarning("foodStack is empty");
            return;
        }

        OrderDishCompareResult comparison = foodStack.Compare(order);
        float fitness = calculator.CalculateFitness(comparison);
        Debug.Log(comparison);
        Debug.LogFormat("Fitness score: {0}, dish: {1} order: {2}", fitness, foodStack, order);
    }
}
