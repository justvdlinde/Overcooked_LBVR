using UnityEngine;
using Utils.Core.Attributes;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private Order order = null;
    [SerializeField] private Dish testDish = null;

    [Button]
    public void EvaluateDish()
    {
        for (int i = 0; i < testDish.ingredients.Count; i++)
        {
            Ingredient ingredient = testDish.ingredients[i];
            Debug.Log(ingredient.ingredientType + " == " + order.ingredients[i] + ": " + (ingredient.ingredientType == order.ingredients[i]));
        }
    }
}
