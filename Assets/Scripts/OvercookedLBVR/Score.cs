using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Score
{
    public readonly Order Order;
    public readonly Dish Dish;
    public bool timerExceeded;

    public float MaxScore => 100;
    public float Points { get; private set; }
    public bool IsPerfectScore { get; private set; }
    public bool IngredientsAreInCorrectOrder { get; private set; }
    public float CorrectIngredientPercentage { get; private set; }
    public float ProperlyCookedIngredientsPercentage { get; private set; }

    public Score(Order order, Dish dish)
    {
        Order = order;
        Dish = dish;
        if(order.timer.TimeRemaining <= 0)
        {
            timerExceeded = true;
            Points = 0;
        }
        else
        {
            Points = CalculatePoints(order, dish);
            timerExceeded = false;
        }
    }

    public override string ToString()
    {
        return string.Format("Score: {0}/{1}, correct order: {2}, correct ingredients: {3}% properly cooked ingredients: {4}%",
            Points, MaxScore, IngredientsAreInCorrectOrder, CorrectIngredientPercentage * 100, ProperlyCookedIngredientsPercentage * 100);
    }

    private float CalculatePoints(Order order, Dish dish)
    {
        int totalIngredients = order.ingredients.Length;
        IngredientsAreInCorrectOrder = true;
        int correctIngredients = 0;
        int properlyCookedIngredients = 0;

        List<IngredientType> dishChecklist = (from i in dish.ingredients select i.ingredientType).ToList();

        int index = 0;
        foreach (IngredientType orderIngredient in order.ingredients)
        {
            // Check for correct order:
            if (IngredientsAreInCorrectOrder && index < dishChecklist.Count && dishChecklist[index] == orderIngredient)
            {
                correctIngredients++;
            }
            // Check for correct ingredients:
            else if (dishChecklist.Contains(orderIngredient))
            {
                // Remove from checklist in case of duplicate ingredients:
                dishChecklist.Remove(orderIngredient);
                correctIngredients++;
                IngredientsAreInCorrectOrder = false;
            }
            else
            {
                IngredientsAreInCorrectOrder = false;
            }

            index++;
        }

        for (int i = 0; i < dish.ingredients.Count; i++)
        {
            Ingredient ingredient = dish.ingredients[i];
            if (ingredient.IsCookedProperly())
                properlyCookedIngredients++;
        }

        //Debug.Log("Ingredients: " + totalIngredients);
        //Debug.Log("correctIngredients: " + correctIngredients);
        //Debug.Log("properly cooked: " + properlyCookedIngredients);
        //Debug.Log("Correct order: " + ingredientsAreInCorrectOrder);

        CorrectIngredientPercentage = Mathf.Lerp(0, 1, (float)correctIngredients / totalIngredients);
        ProperlyCookedIngredientsPercentage = Mathf.Lerp(0, 1, (float)properlyCookedIngredients / totalIngredients);

        //Debug.Log("correctIngredientPercentage: " + correctIngredientPercentage);
        //Debug.Log("properlyCookedIngredientsPercentage: " + properlyCookedIngredientsPercentage);

        float score = MaxScore / 4;
        float totalScore = 0f;
        totalScore += score * CorrectIngredientPercentage;
        totalScore += score * ProperlyCookedIngredientsPercentage;
        if (IngredientsAreInCorrectOrder)
            totalScore += score;

        if (CorrectIngredientPercentage == 1 && ProperlyCookedIngredientsPercentage == 1 && IngredientsAreInCorrectOrder)
        {
            IsPerfectScore = true;
            totalScore += score;
        }

        return totalScore;
    }
}
