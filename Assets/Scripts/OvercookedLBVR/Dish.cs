using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dish : MonoBehaviour
{
    public List<Ingredient> ingredients = new List<Ingredient>();

    public bool ContainsIngredient(IngredientType type)
	{
		foreach (var item in ingredients)
		{
			if (item.ingredientType == type)
				return true;
		}
		return false;
	}
}
